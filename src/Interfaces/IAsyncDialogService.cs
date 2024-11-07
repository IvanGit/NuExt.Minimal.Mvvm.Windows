using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents an asynchronous dialog service that can show dialogs with specified commands and parameters.
    /// </summary>
    public interface IAsyncDialogService
    {
        /// <summary>
        /// Shows a dialog with the specified commands, title, document type, view model, and additional parameters.
        /// </summary>
        /// <param name="dialogCommands">The commands to be displayed in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="documentType">The type of the document associated with the dialog.</param>
        /// <param name="viewModel">The view model associated with the dialog.</param>
        /// <param name="parentViewModel">The parent view model of the view model.</param>
        /// <param name="parameter">An additional parameter for the view model.</param>
        /// <param name="cancellationToken">A token to cancel the dialog operation.</param>
        /// <returns>A <see cref="ValueTask{UICommand}"/> that represents the asynchronous operation. The task result contains a UI command representing the user's action, or null if the dialog was dismissed.</returns>
        ValueTask<UICommand?> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string? title, string? documentType, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shows a dialog with the specified buttons, title, document type, view model, and additional parameters.
        /// </summary>
        /// <param name="dialogButtons">The buttons to be displayed in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="documentType">The type of the document associated with the dialog.</param>
        /// <param name="viewModel">The view model associated with the dialog.</param>
        /// <param name="parentViewModel">The parent view model of the view model.</param>
        /// <param name="parameter">An additional parameter for the view model.</param>
        /// <param name="cancellationToken">A token to cancel the dialog operation.</param>
        /// <returns>A <see cref="ValueTask{MessageBoxResult}"/> that represents the asynchronous operation. The task result contains a MessageBoxResult representing the user's action.</returns>
        ValueTask<MessageBoxResult> ShowDialogAsync(MessageBoxButton dialogButtons, string? title, string? documentType, object? viewModel, object? parentViewModel, object? parameter, CancellationToken cancellationToken = default);
    }
}
