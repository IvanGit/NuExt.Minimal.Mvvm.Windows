using Minimal.Mvvm.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides asynchronous methods to show and manage modal dialogs.
    /// Extends DialogServiceBase and implements IAsyncDialogService interface.
    /// </summary>
    public class InputDialogService : DialogServiceBase, IAsyncDialogService
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="ValidatesOnDataErrors"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatesOnDataErrorsProperty = DependencyProperty.Register(
            nameof(ValidatesOnDataErrors), typeof(bool), typeof(InputDialogService), new PropertyMetadata(false));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the service should check for validation errors
        /// when closing the dialog. If true, the service will prevent the dialog from closing if there are validation errors.
        /// This applies only if the ViewModel implements the <see cref="IDataErrorInfo"/> interface.
        /// </summary>
        public bool ValidatesOnDataErrors
        {
            get => (bool)GetValue(ValidatesOnDataErrorsProperty);
            set => SetValue(ValidatesOnDataErrorsProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays a dialog asynchronously with the specified parameters.
        /// </summary>
        /// <param name="dialogCommands">A collection of UICommand objects representing the buttons available in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="documentType">The type of the view to display within the dialog.</param>
        /// <param name="viewModel">The ViewModel associated with the view.</param>
        /// <param name="parentViewModel">The parent ViewModel for context.</param>
        /// <param name="parameter">The optional parameter for context.</param>
        /// <param name="cancellationToken">A token to cancel the dialog operation if needed.</param>
        /// <returns>A <see cref="ValueTask{UICommand}"/> representing the command selected by the user.</returns>
        public async ValueTask<UICommand?> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string? title, string? documentType, object? viewModel,
            object? parentViewModel, object? parameter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var view = await CreateAndInitializeViewAsync(documentType, viewModel, parentViewModel, parameter, cancellationToken);

            var dialog = new InputDialog
            {
                CommandsSource = dialogCommands,
                Content = view,
                Owner = GetWindow(),
                Title = title ?? (viewModel != null ? ViewModelHelper.GetViewModelTitle(viewModel) : null) ?? string.Empty,
                ValidatesOnDataErrors = ValidatesOnDataErrors,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            return dialog.ShowDialog(cancellationToken);
        }

        /// <summary>
        /// Displays a dialog asynchronously with the specified parameters.
        /// </summary>
        /// <param name="dialogButtons">The buttons to be displayed in the dialog.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="documentType">The type of the view to display within the dialog.</param>
        /// <param name="viewModel">The ViewModel associated with the view.</param>
        /// <param name="parentViewModel">The parent ViewModel for context.</param>
        /// <param name="parameter">The optional parameter for context.</param>
        /// <param name="cancellationToken">A token to cancel the dialog operation if needed.</param>
        /// <returns>A <see cref="ValueTask{MessageBoxResult}"/> representing the user's action.</returns>
        public async ValueTask<MessageBoxResult> ShowDialogAsync(MessageBoxButton dialogButtons, string? title, string? documentType, object? viewModel,
            object? parentViewModel, object? parameter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetMessageBoxResult(await ShowDialogAsync(GetUICommands(dialogButtons), title, documentType, viewModel, parentViewModel, parameter, cancellationToken));
        }

        #endregion
    }
}
