using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a view model for a menu item that can include a command, header, and submenu items.
    /// Inherits from BindableBase to support property change notifications.
    /// </summary>
    public class MenuItemViewModel : BindableBase
    {
        private ICommand? _command;
        private object? _commandParameter;
        private string? _header;
        private ObservableCollection<MenuItemViewModel?>? _subMenuItems;

        #region Properties

        /// <summary>
        /// Gets or sets the command associated with this menu item.
        /// </summary>
        public ICommand? Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        /// <summary>
        /// Gets or sets the parameter to be passed to the command.
        /// </summary>
        public object? CommandParameter
        {
            get => _commandParameter;
            set => SetProperty(ref _commandParameter, value);
        }

        /// <summary>
        /// Gets or sets the header text of the menu item.
        /// </summary>
        public string? Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Gets or sets the collection of submenu items.
        /// </summary>
        public ObservableCollection<MenuItemViewModel?>? SubMenuItems
        {
            get => _subMenuItems;
            set => SetProperty(ref _subMenuItems, value);
        }

        #endregion
    }
}
