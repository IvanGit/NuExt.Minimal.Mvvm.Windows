using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// The <c>DialogServiceBase</c> class is an abstract base class designed for creating services 
    /// that manage dialog interactions in a user interface.
    /// </summary>
    /// <remarks>
    /// This class provides mechanisms for localizing dialog buttons, creating commands based on message box results,
    /// and determining the result of dialogs based on user actions. It simplifies the implementation of dialog-related 
    /// functionalities by offering a consistent approach to handle localization and command creation.
    /// </remarks>
    public abstract class DialogServiceBase : ViewServiceBase<Control>
    {
        #region Dependency Properties

        /// <summary>
        /// Identifies the <see cref="MessageBoxButtonLocalizer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageBoxButtonLocalizerProperty = DependencyProperty.Register(
            nameof(MessageBoxButtonLocalizer), typeof(MessageBoxButtonLocalizerBase), typeof(DialogServiceBase));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the MessageBoxButtonLocalizer used to localize message box buttons.
        /// If set, this localizer will be used to provide localized text for the buttons in the dialog.
        /// </summary>
        public MessageBoxButtonLocalizerBase? MessageBoxButtonLocalizer
        {
            get => (MessageBoxButtonLocalizerBase?)GetValue(MessageBoxButtonLocalizerProperty);
            set => SetValue(MessageBoxButtonLocalizerProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a <see cref="UICommand"/> based on the specified <see cref="MessageBoxResult"/> and localizes its content.
        /// </summary>
        /// <param name="button">The message box button result that identifies the command.</param>
        /// <returns>A new instance of <see cref="UICommand"/> with the specified identifier and localized content.</returns>
        protected UICommand CreateUICommand(MessageBoxResult button)
        {
            return new UICommand() { Id = button, Content = GetMessageBoxButtonLocalizer().Localize(button) };
        }

        /// <summary>
        /// Gets the message box button localizer. If a custom localizer is not set,
        /// the default localizer will be returned.
        /// </summary>
        /// <returns>The instance of <see cref="MessageBoxButtonLocalizerBase"/> used to localize message box buttons.</returns>
        protected MessageBoxButtonLocalizerBase GetMessageBoxButtonLocalizer()
        {
            return MessageBoxButtonLocalizer ?? Windows.MessageBoxButtonLocalizer.Default;
        }

        /// <summary>
        /// Determines the result of a message box based on the provided command.
        /// </summary>
        /// <param name="command">The UI command to extract the result from. This can be null.</param>
        /// <returns>
        /// A <see cref="MessageBoxResult"/> value representing the result of the message box.
        /// If the command is null or its ID does not match any <see cref="MessageBoxResult"/>, the method returns <see cref="MessageBoxResult.None"/>.
        /// </returns>
        protected static MessageBoxResult GetMessageBoxResult(UICommand? command)
        {
            if (command?.Id is MessageBoxResult result)
            {
                return result;
            }
            return MessageBoxResult.None;
        }

        /// <summary>
        /// Generates a collection of <see cref="UICommand"/> objects based on the specified <see cref="MessageBoxButton"/>.
        /// </summary>
        /// <param name="dialogButtons">The type of buttons to include in the message box.</param>
        /// <returns>An enumerable collection of <see cref="UICommand"/> objects representing the buttons in the message box.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported <see cref="MessageBoxButton"/> value is provided.</exception>
        protected virtual IEnumerable<UICommand> GetUICommands(MessageBoxButton dialogButtons)
        {
            var commands = new List<UICommand>();
            UICommand okCommand, cancelCommand, yesCommand, noCommand;
            switch (dialogButtons)
            {
                case MessageBoxButton.OK:
                    okCommand = CreateUICommand(MessageBoxResult.OK);
                    okCommand.IsDefault = true;
                    okCommand.IsCancel = false;
                    commands.Add(okCommand);
                    break;
                case MessageBoxButton.OKCancel:
                    okCommand = CreateUICommand(MessageBoxResult.OK);
                    okCommand.IsDefault = true;
                    okCommand.IsCancel = false;
                    commands.Add(okCommand);

                    cancelCommand = CreateUICommand(MessageBoxResult.Cancel);
                    cancelCommand.IsDefault = false;
                    cancelCommand.IsCancel = true;
                    commands.Add(cancelCommand);
                    break;
                case MessageBoxButton.YesNoCancel:
                    yesCommand = CreateUICommand(MessageBoxResult.Yes);
                    yesCommand.IsDefault = true;
                    yesCommand.IsCancel = false;
                    commands.Add(yesCommand);

                    noCommand = CreateUICommand(MessageBoxResult.No);
                    noCommand.IsDefault = false;
                    noCommand.IsCancel = false;
                    commands.Add(noCommand);

                    cancelCommand = CreateUICommand(MessageBoxResult.Cancel);
                    cancelCommand.IsDefault = false;
                    cancelCommand.IsCancel = true;
                    commands.Add(cancelCommand);
                    break;
                case MessageBoxButton.YesNo:
                    yesCommand = CreateUICommand(MessageBoxResult.Yes);
                    yesCommand.IsDefault = true;
                    yesCommand.IsCancel = false;
                    commands.Add(yesCommand);

                    noCommand = CreateUICommand(MessageBoxResult.No);
                    noCommand.IsDefault = false;
                    noCommand.IsCancel = true;
                    commands.Add(noCommand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogButtons), dialogButtons, null);
            }
            return commands;
        }

        /// <summary>
        /// Attempts to retrieve the Window associated with the current object.
        /// </summary>
        /// <returns>
        /// The Window instance associated with the current object if available; otherwise, null.
        /// </returns>
        protected Window? GetWindow()
        {
            return AssociatedObject != null ? AssociatedObject as Window ?? Window.GetWindow(AssociatedObject) : null;
        }

        #endregion
    }
}
