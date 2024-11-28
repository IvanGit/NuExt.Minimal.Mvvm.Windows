using System.Diagnostics;

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
        /// Closes the window asynchronously, optionally forcing closure.
        /// </summary>
        /// <param name="forceClose">If true, forces the window to close.</param>
        /// <returns>A task representing the asynchronous close operation.</returns>
        public async ValueTask CloseForcedAsync(bool forceClose = true)
        {
            Debug.Assert(CheckAccess());
            Debug.Assert(IsDisposed == false);

            Debug.Assert(CancellationTokenSource.IsCancellationRequested || forceClose);
            if (forceClose)
            {
#if NET8_0_OR_GREATER
                await CancellationTokenSource.CancelAsync();
#else
                CancellationTokenSource.Cancel();
#endif
                WindowPlacementService?.SavePlacement();//TODO check
            }

            try
            {
                await DisposeAsync();

                Debug.Assert(CheckAccess());
                Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
                WindowService?.Close();
                CancellationTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                //TODO logging
                OnError(ex);
            }
        }

        #endregion
    }
}
