namespace System.Windows.Controls
{
    /// <summary>
    /// A DataTemplateSelector for a ComboBox that selects different templates
    /// for the dropdown items and the selected item.
    /// </summary>
    public class ComboBoxItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the DataTemplate used for the dropdown items.
        /// </summary>
        public DataTemplate? DropDownDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the DataTemplate used for the selected item.
        /// </summary>
        public DataTemplate? SelectionDataTemplate { get; set; }

        /// <summary>
        /// Selects the appropriate DataTemplate based on whether the item is in the dropdown or the selection box.
        /// </summary>
        /// <param name="item">The data object being templated.</param>
        /// <param name="container">The container in which the data object is displayed.</param>
        /// <returns>A DataTemplate to apply to the given item.</returns>
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            var comboBoxItem = container.FindParent<ComboBoxItem>();
            return comboBoxItem != null ? DropDownDataTemplate : SelectionDataTemplate;
        }
    }
}
