using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a base class for control-specific ViewModels, extending the functionality of the <see cref="ViewModelBase"/> class.
    /// </summary>
    public partial class ControlViewModel : ViewModelBase, IAsyncDisposable
    {
        public ControlViewModel()
        {
            if (IsInDesignMode)
            {
                return;
            }
            Lifetime.AddBracket(() => PropertyChanged += OnPropertyChanged, () => PropertyChanged -= OnPropertyChanged);
            Lifetime.AddBracket(CreateCommands, NullifyCommands);//last operation after WaitAsyncCommands
            Lifetime.AddAsync(() => WaitAsyncCommands());
        }

        #region Properties

        /// <summary>
        /// Gets or sets the display name of the ViewModel, primarily used for debugging purposes.
        /// </summary>
        public string? DisplayName { get; set; }

        private bool _isDisposed;
        /// <summary>
        /// Gets a value indicating whether the ViewModel has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get => _isDisposed;
            private set
            {
                if (_isDisposed == value) return;
                _isDisposed = value;
                OnPropertyChanged(EventArgsCache.IsDisposedPropertyChanged);
                OnPropertyChanged(EventArgsCache.IsUsablePropertyChanged);
            }
        }

        private bool _isDisposing;
        /// <summary>
        /// Gets a value indicating whether the ViewModel is currently disposing.
        /// </summary>
        public bool IsDisposing
        {
            get => _isDisposing;
            private set
            {
                if (_isDisposing == value) return;
                _isDisposing = value;
                OnPropertyChanged(EventArgsCache.IsDisposingPropertyChanged);
                OnPropertyChanged(EventArgsCache.IsUsablePropertyChanged);
            }
        }

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
            if (e.PropertyName == nameof(IsInitialized))//handle only IsInitialized
            {
                OnPropertyChanged(EventArgsCache.IsUsablePropertyChanged);
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
                throw new ObjectDisposedException(GetType().FullName, message);
            }
        }

        /// <summary>
        /// Asynchronously disposes of the resources used by the instance.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (IsDisposed || IsDisposing)
            {
                return;
            }
            IsDisposing = true;
            try
            {
                ValidateDisposingState();
                await OnDisposeAsync().ConfigureAwait(false);
                ValidateFinalState();
                IsDisposed = true;
            }
            catch (Exception ex)
            {
                Debug.Assert(false, $"{GetType().FullName} ({DisplayName ?? "Unnamed"}) ({GetHashCode()}):{Environment.NewLine}{ex.Message}");
                throw;
            }
            finally
            {
                IsDisposing = false;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual ValueTask OnDisposeAsync()
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
            Debug.Assert(CheckAccess());
            Trace.WriteLine($"An error has occurred in {callerName}:{Environment.NewLine}{ex.Message}");
        }

        /// <inheritdoc />
        protected override async Task OnUninitializeAsync(CancellationToken cancellationToken)
        {
            await DisposeAsync().ConfigureAwait(false);
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
