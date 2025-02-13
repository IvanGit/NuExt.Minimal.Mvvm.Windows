﻿using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using static AccessModifier;

namespace Minimal.Mvvm.Windows
{
    partial class WindowViewModel
    {
        #region Commands

        /// <summary>
        /// Gets or sets the command that is executed when the content is rendered.
        /// </summary>
        [Notify(Setter = Private)]
        private ICommand? _contentRenderedCommand;

        /// <summary>
        /// Gets or sets the command that is executed to close the window.
        /// </summary>
        [Notify(Setter = Private)]
        private ICommand? _closeCommand;

        /// <summary>
        /// Gets or sets the command that is executed during the closing event of the window.
        /// </summary>
        [Notify(Setter = Private)]
        private ICommand? _closingCommand;

        /// <summary>
        /// Gets or sets the command that is executed after the window placement has been restored.
        /// </summary>
        [Notify(Setter = Private)]
        private ICommand? _placementRestoredCommand;

        /// <summary>
        /// Gets or sets the command that is executed after the window placement has been saved.
        /// </summary>
        [Notify(Setter = Private)]
        private ICommand? _placementSavedCommand;

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
        /// Handles the closing event of the window, managing cancellation and disposal states.
        /// </summary>
        /// <param name="arg">The arguments for the cancel event.</param>
        private void Closing(CancelEventArgs arg)
        {
            //https://weblog.west-wind.com/posts/2019/Sep/02/WPF-Window-Closing-Errors
            if (arg.Cancel)
            {
                return;
            }

            if (CancellationTokenSource.IsCancellationRequested || IsDisposed)
            {
                arg.Cancel = IsDisposing;//do not close while disposing
                return;
            }
            arg.Cancel = true;
            Dispatcher.InvokeAsync(async () => { await CloseAsync(false); });
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

        /// <inheritdoc />
        protected override void CreateCommands()
        {
            base.CreateCommands();

            ContentRenderedCommand = RegisterAsyncCommand(ContentRenderedAsync);
            CloseCommand = RegisterCommand(Close);
            ClosingCommand = RegisterCommand<CancelEventArgs>(Closing);
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
