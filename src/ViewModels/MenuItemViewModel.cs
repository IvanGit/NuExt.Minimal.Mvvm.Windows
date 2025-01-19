using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a view model for a menu item that can include a command, header, and submenu items.
    /// Inherits from BindableBase to support property change notifications.
    /// </summary>
    public partial class MenuItemViewModel : BindableBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the command associated with this menu item.
        /// </summary>
        [Notify]
        private ICommand? _command;

        /// <summary>
        /// Gets or sets the parameter to be passed to the command.
        /// </summary>
        [Notify]
        private object? _commandParameter;

        /// <summary>
        /// Gets or sets the header text of the menu item.
        /// </summary>
        [Notify]
        private string? _header;

        /// <summary>
        /// Gets or sets the collection of submenu items.
        /// </summary>
        [Notify]
        private ObservableCollection<MenuItemViewModel?>? _subMenuItems;

        #endregion
    }
}
