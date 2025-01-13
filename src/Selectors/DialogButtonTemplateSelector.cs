using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// A template selector for dialog buttons that selects a DataTemplate based on whether the UICommand is marked as default.
    /// </summary>
    public sealed class DialogButtonTemplateSelector : DataTemplateSelector
    {
        #region Properties

        /// <summary>
        /// Gets or sets the template used for non-default buttons.
        /// </summary>
        public DataTemplate? ButtonTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template used for default buttons.
        /// </summary>
        public DataTemplate? DefaultButtonTemplate { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Selects a data template based on the item and the container.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound element.</param>
        /// <returns>
        /// The <see cref="DataTemplate"/> to apply, or null if no template is required.
        /// </returns>
        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            return item is UICommand { IsDefault: true } ? DefaultButtonTemplate ?? ButtonTemplate : ButtonTemplate;
        }

        #endregion
    }
}
