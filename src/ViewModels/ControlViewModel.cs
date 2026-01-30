using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class for control-specific ViewModels, extending the functionality of the <see cref="ViewModelBase"/> class.
    /// </summary>
    public partial class ControlViewModel : ViewModelBase, IControlViewModel, IAsyncDisposable
    {
#if NET9_0_OR_GREATER
        private enum States
        {
            NotDisposed,// default value of _state
            Disposing,
            Disposed
        }

        private volatile States _state;
#else
        private class States
        {
            public const int NotDisposed = 0;// default value of _state
            public const int Disposing = 1;
            public const int Disposed = 2;
        }

        private volatile int _state;
#endif

        public ControlViewModel()
        {
            if (IsInDesignMode)
            {
                return;
            }
            Lifetime.AddBracket(() => PropertyChanged += OnPropertyChanged, () => PropertyChanged -= OnPropertyChanged);
            Lifetime.AddBracket(CreateCommands, NullifyCommands);//operation after WaitAsyncCommands
            Lifetime.AddAsync(() => WaitAsyncCommands());
        }

        #region Properties

        /// <summary>
        /// Gets the dispatcher associated with the UI thread.
        /// </summary>
        public Dispatcher Dispatcher { get; } = Dispatcher.CurrentDispatcher;

        private static bool? s_isInDesignMode;
        /// <summary>
        /// Gets a value indicating whether the ViewModel is in design mode.
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                s_isInDesignMode ??= (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;
                return s_isInDesignMode.Value;
            }
        }

        /// <summary>
        /// Gets or sets the display name of the ViewModel, primarily used for debugging purposes.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the ViewModel has been disposed.
        /// </summary>
        public bool IsDisposed => _state == States.Disposed;

        /// <summary>
        /// Gets a value indicating whether the ViewModel is currently disposing.
        /// </summary>
        public bool IsDisposing => _state == States.Disposing;

        /// <summary>
        /// Gets a value indicating whether the object is usable.
        /// </summary>
        /// <remarks>
        /// The object is considered usable if it has been initialized and 
        /// is neither in the process of being disposed nor already disposed.
        /// This property ensures that the object is in a valid state for operations.
        /// </remarks>
        public bool IsUsable => IsInitialized && IsDisposed == false && IsDisposing == false;

        /// <summary>
        /// Gets the contract for managing the asynchronous lifecycle of resources and actions.
        /// </summary>
        protected AsyncLifetime Lifetime { get; } = new(continueOnCapturedContext: true);

        #endregion

        #region Event Handlers

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(ReferenceEquals(sender, this));

            switch (e.PropertyName)
            {
                case nameof(IsDisposing) when IsDisposing:
                    CancelAsyncCommands();
                    goto case nameof(IsDisposing);
                case nameof(IsInitialized):
                case nameof(IsDisposed):
                case nameof(IsDisposing):
                    OnPropertyChanged(EventArgsCache.IsUsablePropertyChanged);
                    break;

            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the object has been disposed and throws an <see cref="ObjectDisposedException"/> if it has.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
            if (IsDisposed)
            {
                var message = $"{GetType().FullName} ({DisplayName ?? "Unnamed"}) ({GetHashCode()}) has been disposed.";
                Trace.WriteLine(message);
                Debug.Fail(message);
                Throw.ObjectDisposedException(this, message);
            }
        }

        /// <summary>
        /// Asynchronously disposes of the resources used by the instance.
        /// This method must be called from the UI thread (same thread where the instance was created).
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            Debug.Assert(CheckAccess());
            VerifyAccess();

            if (_state != States.NotDisposed)
            {
                // Already disposing or disposed
                return;
            }
            _state = States.Disposing;

            OnPropertyChanged(EventArgsCache.IsDisposingPropertyChanged);
            try
            {
                ValidateDisposingState();
                //https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
                await DisposeAsyncCore().ConfigureAwait(false);
                ValidateFinalState();
            }
            catch (Exception ex)
            {
                string errorMessage = $"{GetType().FullName} ({DisplayName ?? "Unnamed"}) ({GetHashCode()}):{Environment.NewLine}{ex.Message}";
                Trace.WriteLine(errorMessage);
                Debug.Fail(errorMessage);
                throw;
            }
            finally
            {
                _state = States.Disposed;
                OnPropertyChanged(EventArgsCache.IsDisposingPropertyChanged);
                OnPropertyChanged(EventArgsCache.IsDisposedPropertyChanged);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual ValueTask DisposeAsyncCore()
        {
            return Lifetime.DisposeAsync();
        }

        /// <summary>
        /// Handles errors that occur within the ViewModel, providing a mechanism to display error messages.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="callerName">The name of the calling method (automatically provided).</param>
        protected virtual void OnError(Exception ex, [CallerMemberName] string? callerName = null)
        {
            Trace.WriteLine($"An error has occurred in {callerName}:{Environment.NewLine}{ex.Message}");
        }

        /// <summary>
        /// When overridden in a derived class, asynchronously performs the uninitialization logic for the ViewModel.
        /// This method always calls <see cref="DisposeAsync"/> to ensure proper resource cleanup,
        /// regardless of the cancellation state.
        /// </summary>
        protected override Task UninitializeAsyncCore(CancellationToken cancellationToken)
        {
            return InvokeAsync(() => DisposeAsync().AsTask());
        }

        #endregion
    }

    internal static partial class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs IsDisposedPropertyChanged = new(nameof(ControlViewModel.IsDisposed));
        internal static readonly PropertyChangedEventArgs IsDisposingPropertyChanged = new(nameof(ControlViewModel.IsDisposing));
        internal static readonly PropertyChangedEventArgs IsUsablePropertyChanged = new(nameof(ControlViewModel.IsUsable));
    }
}
