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
    public partial class WindowViewModel : ControlViewModel
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> used for managing cancellation of asynchronous operations.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; } = new();

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
        protected OpenWindowsService? OpenWindowsService => GetService<OpenWindowsService>();

        /// <summary>
        /// Gets the service responsible for managing window placement.
        /// </summary>
        protected WindowPlacementService? WindowPlacementService => GetService<WindowPlacementService>();

        /// <summary>
        /// Gets the service responsible for managing the current window.
        /// </summary>
        protected WindowService? WindowService => GetService<WindowService>();

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the window can be closed. Override this method to provide custom close logic.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the window can be closed; otherwise, false.</returns>
        protected virtual ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(true);
        }

        /// <summary>
        /// Closes the window by calling the current window service.
        /// </summary>
        private void Close()
        {
            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            WindowService?.Close();
        }

        /// <summary>
        /// Closes the window asynchronously, optionally forcing closure.
        /// </summary>
        /// <param name="force">If true, forces the window to close.</param>
        /// <returns>A task representing the asynchronous close operation.</returns>
        public async ValueTask CloseAsync(bool force = true)
        {
            Debug.Assert(CheckAccess());
            Debug.Assert(IsDisposed == false);

            VerifyAccess();
            CheckDisposed();

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
                        if (await CanCloseAsync(CancellationTokenSource.Token) == false)
                        {
                            return;
                        }
                    }
                    catch (OperationCanceledException) when (CancellationTokenSource.IsCancellationRequested)
                    {
                        //do nothing and return
                        return;
                    }
                }

#if NET8_0_OR_GREATER
                await CancellationTokenSource.CancelAsync();
#else
                CancellationTokenSource.Cancel();
#endif
                Hide();
                await DisposeAsync();

                Debug.Assert(CheckAccess());
                Close();
                CancellationTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void Hide()
        {
            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            WindowService?.Hide();
        }

        #endregion
    }
}
