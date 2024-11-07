using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    partial class WindowViewModel
    {
        #region Commands

        private ICommand? _contentRenderedCommand;
        /// <summary>
        /// Gets or sets the command that is executed when the content is rendered.
        /// </summary>
        public ICommand? ContentRenderedCommand
        {
            get => _contentRenderedCommand;
            private set => SetProperty(ref _contentRenderedCommand, value);
        }

        private ICommand? _closeCommand;
        /// <summary>
        /// Gets or sets the command that is executed to close the window.
        /// </summary>
        public ICommand? CloseCommand
        {
            get => _closeCommand;
            private set => SetProperty(ref _closeCommand, value);
        }

        private ICommand? _closingCommand;
        /// <summary>
        /// Gets or sets the command that is executed during the closing event of the window.
        /// </summary>
        public ICommand? ClosingCommand
        {
            get => _closingCommand;
            private set => SetProperty(ref _closingCommand, value);
        }

        private ICommand? _placementRestoredCommand;
        /// <summary>
        /// Gets or sets the command that is executed after the window placement has been restored.
        /// </summary>
        public ICommand? PlacementRestoredCommand
        {
            get => _placementRestoredCommand;
            private set => SetProperty(ref _placementRestoredCommand, value);
        }

        private ICommand? _placementSavedCommand;
        /// <summary>
        /// Gets or sets the command that is executed after the window placement has been saved.
        /// </summary>
        public ICommand? PlacementSavedCommand
        {
            get => _placementSavedCommand;
            private set => SetProperty(ref _placementSavedCommand, value);
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Asynchronously executes operations when the content of the window is rendered.
        /// </summary>
        private async Task ContentRenderedAsync()
        {
            try
            {
                await OnContentRenderedAsync(CancellationTokenSource.Token);
            }
            catch (OperationCanceledException ex)
            {
                if (CancellationTokenSource.IsCancellationRequested == false)
                {
                    Debug.Fail(ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                //TODO logging
                if (CancellationTokenSource.IsCancellationRequested == false)
                {
                    OnError(ex);
                }
            }

            if (CancellationTokenSource.IsCancellationRequested) return;

            var openWindowsService = OpenWindowsService;
            if (openWindowsService != null)
            {
                Lifetime.AddBracket(() => openWindowsService.Register(this), () => openWindowsService.Unregister(this));
            }
        }

        /// <summary>
        /// Determines whether the window can be closed. Override this method to provide custom close logic.
        /// </summary>
        /// <returns>True if the window can be closed; otherwise, false.</returns>
        protected virtual bool CanClose()
        {
            return true;
        }

        /// <summary>
        /// Closes the window by calling the current window service.
        /// </summary>
        public virtual void Close()
        {
            WindowService?.Close();
        }

        /// <summary>
        /// Handles the closing event of the window, managing cancellation and disposal states.
        /// </summary>
        /// <param name="arg">The arguments for the cancel event.</param>
        private void Closing(CancelEventArgs arg)
        {
            //https://weblog.west-wind.com/posts/2019/Sep/02/WPF-Window-Closing-Errors
            if (CancellationTokenSource.IsCancellationRequested || IsDisposed)
            {
                Debug.Assert(arg.Cancel == false);
                arg.Cancel = IsDisposing;//do not close while disposing
                return;
            }
            if (CanClose() == false)
            {
                arg.Cancel = false;
                return;
            }
            CancellationTokenSource.Cancel();
            arg.Cancel = true;
            //Debug.Assert(Dispatcher != null, $"{nameof(Dispatcher)} is null");
            Dispatcher.BeginInvoke(async () => { await CloseForcedAsync(false); });
        }

        /// <summary>
        /// This method is called after the window placement has been restored.
        /// Override this method to add custom logic that should run after placement is restored.
        /// </summary>
        protected virtual void OnPlacementRestored()
        {

        }

        /// <summary>
        /// This method is called after the window placement has been saved.
        /// Override this method to add custom logic that should run after placement is saved.
        /// </summary>
        protected virtual void OnPlacementSaved()
        {

        }

        #endregion

        #region Methods

        protected override void CreateCommands()
        {
            base.CreateCommands();

            ContentRenderedCommand = RegisterAsyncCommand(ContentRenderedAsync);
            CloseCommand = RegisterCommand(Close, CanClose);
            ClosingCommand = RegisterCommand<CancelEventArgs>(Closing!);
            PlacementRestoredCommand = RegisterCommand(OnPlacementRestored);
            PlacementSavedCommand = RegisterCommand(OnPlacementSaved);
        }

        /// <summary>
        /// Called when the content of the window is rendered.
        /// Allows for additional initialization or setup that depends on the window's content being ready.
        /// </summary>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual ValueTask OnContentRenderedAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(WindowService != null, $"{nameof(WindowService)} is null");
            Debug.Assert(OpenWindowsService != null, $"{nameof(OpenWindowsService)} is null");
            Debug.Assert(WindowPlacementService != null, $"{nameof(WindowPlacementService)} is null");
            return default;
        }

        #endregion
    }
}
