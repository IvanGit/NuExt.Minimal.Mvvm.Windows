using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a UI command with associated properties for use in various scenarios,
    /// such as binding to buttons in a WPF application.
    /// </summary>
    public partial class UICommand : BindableBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the command is a cancel action.
        /// </summary>
        [Notify]
        private bool _isCancel;

        /// <summary>
        /// Gets or sets the command to be executed.
        /// </summary>
        [Notify] 
        private ICommand? _command;

        /// <summary>
        /// Gets or sets the content displayed on the UI element, such as the text of a button.
        /// </summary>
        [Notify] 
        private object? _content;

        /// <summary>
        /// Gets or sets the identifier for the command.
        /// </summary>
        [Notify] 
        private object? _id;

        /// <summary>
        /// Gets or sets a value indicating whether the command is a default action.
        /// </summary>
        [Notify] 
        private bool _isDefault;

        /// <summary>
        /// Gets or sets the tag property.
        /// </summary>
        [Notify] 
        private object? _tag;

        #endregion
    }
}
