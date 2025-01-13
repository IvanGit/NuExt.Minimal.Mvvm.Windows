using System.Diagnostics;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public static class ComboBoxItemHelper
    {
        /// <summary>
        /// Retrieves a ComboBoxItem from a ComboBox based on the specified item.
        /// </summary>
        /// <param name="container">The ComboBox from which to retrieve the item.</param>
        /// <param name="item">The item whose corresponding ComboBoxItem is to be retrieved.</param>
        /// <returns>The matching ComboBoxItem if found; otherwise, null.</returns>
        public static ComboBoxItem? GetComboBoxItem(this ComboBox? container, object item)
        {
            if (container == null) return null;

            container.ApplyTemplate();

            var itemsPresenter = (ItemsPresenter?)container.Template.FindName("ItemsPresenter", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter,
                // so walk the descendents and find the child.
                itemsPresenter = container.FindChild<ItemsPresenter>();
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();

                    itemsPresenter = container.FindChild<ItemsPresenter>();
                }
            }

            Debug.Assert(itemsPresenter != null);
            if (itemsPresenter == null) return null;

            var count = VisualTreeHelper.GetChildrenCount(itemsPresenter);
            if (count == 0) return null;

            Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

#pragma warning disable IDE0059
            // Ensure that the generator for this panel has been created.
            UIElementCollection children = itemsHostPanel.Children;
#pragma warning restore IDE0059

#pragma warning disable IDE0019
            VirtualizingStackPanel? virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;
#pragma warning restore IDE0019

            count = container.Items.Count;
            for (int i = 0; i < count; i++)
            {
                ComboBoxItem? subContainer;
                if (virtualizingPanel != null)
                {
                    // Bring the item into view so
                    // that the container will be generated.
                    virtualizingPanel.BringIntoView(i);

                    subContainer =
                        (ComboBoxItem?)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                }
                else
                {
                    subContainer =
                        (ComboBoxItem?)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                    // Bring the item into view to maintain the
                    // same behavior as with a virtualizing panel.
                    subContainer?.BringIntoView();
                }

                if (subContainer != null)
                {
                    if (subContainer.DataContext == item)
                    {
                        return subContainer;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all ComboBoxItem elements from a ComboBox, accounting for both virtualized and non-virtualized panels.
        /// </summary>
        /// <param name="container">The ComboBox from which to retrieve the items.</param>
        /// <returns>A collection of ComboBoxItem elements, or null if the container is null or no items could be retrieved.</returns>
        /// <remarks>
        /// This method ensures that all items are brought into view in case of virtualization,
        /// and retrieves the corresponding ComboBoxItem containers from the ItemContainerGenerator.
        /// </remarks>
        public static IEnumerable<ComboBoxItem>? GetComboBoxItems(this ComboBox? container)
        {
            if (container == null) return null;

            container.ApplyTemplate();

            var itemsPresenter = (ItemsPresenter?)container.Template.FindName("ItemsPresenter", container);
            if (itemsPresenter != null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                // The Tree template has not named the ItemsPresenter,
                // so walk the descendents and find the child.
                itemsPresenter = container.FindChild<ItemsPresenter>();
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();

                    itemsPresenter = container.FindChild<ItemsPresenter>();
                }
            }

            Debug.Assert(itemsPresenter != null);
            if (itemsPresenter == null) return null;

            var count = VisualTreeHelper.GetChildrenCount(itemsPresenter);
            if (count == 0) return null;

            Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

#pragma warning disable IDE0059
            // Ensure that the generator for this panel has been created.
            UIElementCollection children = itemsHostPanel.Children;
#pragma warning restore IDE0059

#pragma warning disable IDE0019
            VirtualizingStackPanel? virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;
#pragma warning restore IDE0019

            var items = new List<ComboBoxItem>();
            count = container.Items.Count;
            for (int i = 0; i < count; i++)
            {
                ComboBoxItem? subContainer;
                if (virtualizingPanel != null)
                {
                    // Bring the item into view so
                    // that the container will be generated.
                    virtualizingPanel.BringIntoView(i);

                    subContainer =
                        (ComboBoxItem?)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                }
                else
                {
                    subContainer =
                        (ComboBoxItem?)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                    // Bring the item into view to maintain the
                    // same behavior as with a virtualizing panel.
                    subContainer?.BringIntoView();
                }

                if (subContainer != null)
                {
                    items.Add(subContainer);
                }
            }

            return items;
        }
    }
}
