using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a ViewModel for a window, providing properties and methods for managing the window's state,
    /// services for handling various window-related operations, and commands for interacting with the UI.
    /// </summary>
    public partial class WindowViewModel : ControlViewModel, IWindowViewModel
    {
        private readonly CancellationTokenSource _cts = new();
        private bool _isClosing;

        #region Properties

        /// <summary>
        /// Gets the cancellation token for this window's lifecycle.
        /// </summary>
        public CancellationToken CancellationToken => _cts.Token;

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        [Notify]
        private string? _title;

        #endregion

        #region Services

        /// <summary>
        /// Gets the service responsible for managing open windows.
        /// </summary>
        protected IOpenWindowsService? OpenWindowsService => GetService<IOpenWindowsService>();

        /// <summary>
        /// Gets the service responsible for managing window placement.
        /// </summary>
        protected IWindowPlacementService? WindowPlacementService => GetService<IWindowPlacementService>();

        /// <summary>
        /// Gets the service responsible for managing the current window.
        /// </summary>
        protected IWindowService? WindowService => GetService<IWindowService>();

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the window can be closed. Override this method to provide custom close logic.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the window can be closed; otherwise, false.</returns>
        protected virtual ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? ValueTask.FromCanceled<bool>(cancellationToken) : ValueTask.FromResult(true);
        }

        /// <summary>
        /// Closes the window by calling the current window service.
        /// </summary>
        private void Close()
        {
            Debug.Assert(CheckAccess());
            VerifyAccess();

            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            WindowService?.Close();
        }

        /// <summary>
        /// Asynchronously closes the window and disposes the ViewModel if closure is not canceled. 
        /// A disposed ViewModel should not be reused.
        /// </summary>
        /// <param name="force">
        /// <see langword="true"/> to close immediately; 
        /// <see langword="false"/> to allow cancellation via <see cref="CanCloseAsync"/>.
        /// </param>
        public async ValueTask CloseAsync(bool force = true)
        {
            Debug.Assert(CheckAccess());
            Debug.Assert(IsDisposed == false);

            VerifyAccess();
            CheckDisposed();

            if (_isClosing)
            {
                return;
            }

            _isClosing = true;
            try
            {
                if (force)
                {
                    WindowPlacementService?.SavePlacement();//TODO check
                }
                else
                {
                    try
                    {
                        if (await CanCloseAsync(CancellationToken) == false)
                        {
                            return;
                        }
                    }
                    catch (OperationCanceledException) when (CancellationToken.IsCancellationRequested)
                    {
                        //do nothing and return
                        return;
                    }
                }

#if NET8_0_OR_GREATER
                await _cts.CancelAsync();
#else
                _cts.Cancel();
#endif
                Hide();
                await DisposeAsync();

                Debug.Assert(CheckAccess());
                Close();
                _cts.Dispose();
            }
            catch (Exception ex)
            {
                OnError(ex);
                if (force)
                {
                    throw;
                }
            }
            finally
            {
                _isClosing = false;
            }
        }

        private void Hide()
        {
            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            WindowService?.Hide();
        }

        /// <inheritdoc/>
        protected override Task InitializeAsyncCore(CancellationToken cancellationToken)
        {
            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            Debug.Assert(OpenWindowsService != null, $"{nameof(OpenWindowsService)} is null");
            Debug.Assert(WindowPlacementService != null, $"{nameof(WindowPlacementService)} is null");
            return base.InitializeAsyncCore(cancellationToken);
        }

        #endregion
    }
}
