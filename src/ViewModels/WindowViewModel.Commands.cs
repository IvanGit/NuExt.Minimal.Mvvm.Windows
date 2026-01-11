using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
        /// <remarks>
        /// This command should be bound to the window in the view to handle the ContentRendered event. For example:
        /// <code>
        /// &lt;minimal:EventTrigger EventName="ContentRendered" Command="{Binding ContentRenderedCommand}" /&gt;
        /// </code>
        /// The command allows you to perform actions such as initial data loading in the <see cref="ContentRenderedAsync"/> method after the window's content has been fully rendered.
        /// </remarks>
        [Notify(Setter = Private)]
        private ICommand? _contentRenderedCommand;

        /// <summary>
        /// Gets or sets the command that is executed to close the window.
        /// </summary>
        /// <remarks>
        /// This command can be bound to a control in the view to handle the window closing action. For example:
        /// <code>
        /// &lt;Button Content="Close" Command="{Binding CloseCommand}" /&gt;
        /// </code>
        /// </remarks>
        [Notify(Setter = Private)]
        private ICommand? _closeCommand;

        /// <summary>
        /// Gets or sets the command that is executed during the closing event of the window.
        /// </summary>
        /// <remarks>
        /// This command should be bound to a window in the view to handle the window's Closing event. For example:
        /// <code>
        /// &lt;minimal:EventTrigger EventName="Closing" Command="{Binding ClosingCommand}" PassEventArgsToCommand="True" /&gt;
        /// </code>
        /// The command allows you to manage cancellation in the <see cref="CanCloseAsync"/> method and perform cleanup operations in the <see cref="ControlViewModel.DisposeAsyncCore"/> method when the window is closing.
        /// Note that <c>PassEventArgsToCommand</c> must be specified and set to <c>True</c> to pass the <see cref="System.ComponentModel.CancelEventArgs"/> to the command.
        /// </remarks>
        [Notify(Setter = Private)]
        private ICommand? _closingCommand;

        /// <summary>
        /// Gets or sets the command that is executed after the window placement has been restored.
        /// </summary>
        /// <remarks>
        /// This command should be bound to the <see cref="Minimal.Mvvm.Windows.WindowPlacementService"/> in the view to handle actions after the window's placement has been restored. For example:
        /// <code>
        /// &lt;minimal:WindowPlacementService FileName="MainWindow" 
        ///                                  DirectoryName="{Binding EnvironmentService.SettingsDirectory, FallbackValue={x:Null}}"
        ///                                  PlacementRestoredCommand="{Binding PlacementRestoredCommand}" 
        ///                                  PlacementSavedCommand="{Binding PlacementSavedCommand}"/&gt;
        /// </code>
        /// </remarks>
        [Notify(Setter = Private)]
        private ICommand? _placementRestoredCommand;

        /// <summary>
        /// Gets or sets the command that is executed after the window placement has been saved.
        /// </summary>
        /// <remarks>
        /// This command should be bound to the <see cref="Minimal.Mvvm.Windows.WindowPlacementService"/> in the view to handle actions after the window's placement has been saved. For example:
        /// <code>
        /// &lt;minimal:WindowPlacementService FileName="MainWindow" 
        ///                                  DirectoryName="{Binding EnvironmentService.SettingsDirectory, FallbackValue={x:Null}}"
        ///                                  PlacementRestoredCommand="{Binding PlacementRestoredCommand}" 
        ///                                  PlacementSavedCommand="{Binding PlacementSavedCommand}"/&gt;
        /// </code>
        /// </remarks>
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
                await OnContentRenderedAsync(CancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                if (CancellationToken.IsCancellationRequested == false)
                {
                    Debug.Fail(ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (CancellationToken.IsCancellationRequested == false)
                {
                    OnError(ex);
                }
            }

            if (CancellationToken.IsCancellationRequested) return;

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
            if (arg.Cancel || _isClosing)
            {
                return;
            }

            if (CancellationToken.IsCancellationRequested)
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

            ContentRenderedCommand = new AsyncCommand(ContentRenderedAsync);
            CloseCommand = new RelayCommand(Close);
            ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            PlacementRestoredCommand = new RelayCommand(OnPlacementRestored);
            PlacementSavedCommand = new RelayCommand(OnPlacementSaved);
        }

        /// <inheritdoc />
        protected override ICommand? GetCurrentCommand([CallerMemberName] string? callerName = null)
        {
            return callerName switch
            {
                nameof(ContentRenderedAsync) => ContentRenderedCommand,
                nameof(Close) => CloseCommand,
                nameof(Closing) => ClosingCommand,
                nameof(OnPlacementRestored) => PlacementRestoredCommand,
                nameof(OnPlacementSaved) => PlacementSavedCommand,
                _ => base.GetCurrentCommand(callerName)
            };
        }

        /// <inheritdoc />
        protected override void GetAllCommands(ref ValueListBuilder<(string PropertyName, ICommand? Command)> builder)
        {
            base.GetAllCommands(ref builder);

            builder.Append((nameof(ContentRenderedCommand), ContentRenderedCommand));
            builder.Append((nameof(CloseCommand), CloseCommand));
            builder.Append((nameof(ClosingCommand), ClosingCommand));
            builder.Append((nameof(PlacementRestoredCommand), PlacementRestoredCommand));
            builder.Append((nameof(PlacementSavedCommand), PlacementSavedCommand));
        }

        /// <inheritdoc />
        protected override void NullifyCommands()
        {
            ContentRenderedCommand = null;
            CloseCommand = null;
            ClosingCommand = null;
            PlacementRestoredCommand = null;
            PlacementSavedCommand = null;

            base.NullifyCommands();
        }

        /// <summary>
        /// Called when the content of the window is rendered.
        /// Allows for additional initialization or setup that depends on the window's content being ready.
        /// </summary>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// The ContentRenderedCommand should be bound to the window in the view to handle the ContentRendered event. For example:
        /// <code>
        /// &lt;minimal:EventTrigger EventName="ContentRendered" Command="{Binding ContentRenderedCommand}" /&gt;
        /// </code>
        /// The command allows you to perform actions such as initial data loading in the <see cref="ContentRenderedAsync"/> method after the window's content has been fully rendered.
        /// </remarks>
        protected virtual ValueTask OnContentRenderedAsync(CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? ValueTask.FromCanceled(cancellationToken) : ValueTask.CompletedTask;
        }

        #endregion
    }
}
