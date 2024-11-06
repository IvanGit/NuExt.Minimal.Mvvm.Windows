using System.Diagnostics;
using System.Windows.Media;

//How to: Find a TreeViewItem in a TreeView
//https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview?view=netframeworkdesktop-4.8

namespace System.Windows.Controls
{
    public static class TreeViewItemHelper
    {
        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        public static TreeViewItem? GetTreeViewItem(this ItemsControl? container, object item)
        {
            if (container == null) return null;

            if (container.DataContext == item)
            {
                return container as TreeViewItem;
            }

            // Expand the current container
            if (container is TreeViewItem { IsExpanded: false } viewItem)
            {
                viewItem.SetValue(TreeViewItem.IsExpandedProperty, true);
            }

            // Try to generate the ItemsPresenter and the ItemsPanel.
            // by calling ApplyTemplate.  Note that in the
            // virtualizing case even if the item is marked
            // expanded we still need to do this step in order to
            // regenerate the visuals because they may have been virtualized away.

            container.ApplyTemplate();
            var itemsPresenter = (ItemsPresenter?)container.Template.FindName("ItemsHost", container);
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

            Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

#pragma warning disable IDE0059
            // Ensure that the generator for this panel has been created.
            UIElementCollection children = itemsHostPanel.Children;
#pragma warning restore IDE0059

#pragma warning disable IDE0019
            VirtualizingStackPanel? virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;
#pragma warning restore IDE0019

            for (int i = 0, count = container.Items.Count; i < count; i++)
            {
                TreeViewItem? subContainer;
                if (virtualizingPanel != null)
                {
                    // Bring the item into view so
                    // that the container will be generated.
                    virtualizingPanel.BringIntoView(i);

                    subContainer =
                        (TreeViewItem?)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                }
                else
                {
                    subContainer =
                        (TreeViewItem?)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                    // Bring the item into view to maintain the
                    // same behavior as with a virtualizing panel.
                    subContainer?.BringIntoView();
                }

                if (subContainer != null)
                {
                    // Search the next level for the object.
                    TreeViewItem? resultContainer = GetTreeViewItem(subContainer, item);
                    if (resultContainer != null)
                    {
                        return resultContainer;
                    }
                    else
                    {
                        // The object is not under this TreeViewItem
                        // so collapse it.
                        subContainer.IsExpanded = false;
                    }
                }
            }

            return null;
        }
    }
}
