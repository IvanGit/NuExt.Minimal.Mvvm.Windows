using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a UI command with associated properties for use in various scenarios,
    /// such as binding to buttons in a WPF application.
    /// </summary>
    public class UICommand : BindableBase
    {
        #region Properties

        private bool _isCancel;
        /// <summary>
        /// Gets or sets a value indicating whether the command is a cancel action.
        /// </summary>
        public bool IsCancel
        {
            get => _isCancel;
            set => SetProperty(ref _isCancel, value);
        }

        private ICommand? _command;
        /// <summary>
        /// Gets or sets the command to be executed.
        /// </summary>
        public ICommand? Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        private object? _content;
        /// <summary>
        /// Gets or sets the content displayed on the UI element, such as the text of a button.
        /// </summary>
        public object? Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private object? _id;
        /// <summary>
        /// Gets or sets the identifier for the command.
        /// </summary>
        public object? Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private bool _isDefault;
        /// <summary>
        /// Gets or sets a value indicating whether the command is a default action.
        /// </summary>
        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        private object? _tag;
        /// <summary>
        /// Gets or sets the tag property.
        /// </summary>
        public object? Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }

        #endregion
    }
}
