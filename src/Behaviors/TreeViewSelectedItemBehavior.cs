using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    public class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(TreeViewSelectedItemBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        private bool _inSelectedItemChanged;

        #region Properties

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        #endregion

        #region Event Handlers

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
            {
                return;
            }
            if (sender is not TreeViewSelectedItemBehavior behavior) return;

            if (behavior._inSelectedItemChanged)
            {
                return;
            }

            var tree = behavior.AssociatedObject;
            if (tree == null) return;

            if (e.NewValue == null)
            {
                foreach (var item in tree.Items.OfType<TreeViewItem>())
                {
                    item.SetValue(TreeViewItem.IsSelectedProperty, false);
                }
                return;
            }

            if (e.NewValue is TreeViewItem treeViewItem)
            {
                treeViewItem.SetValue(TreeViewItem.IsSelectedProperty, true);
                //treeViewItem.Focus();
                return;
            }

            var dataBoundTreeViewItem = tree.GetTreeViewItem(e.NewValue);
            Debug.Assert(dataBoundTreeViewItem != null);
            if (dataBoundTreeViewItem == null) return;
            dataBoundTreeViewItem.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _inSelectedItemChanged = true;
            try
            {
                SelectedItem = e.NewValue;
            }
            finally
            {
                _inSelectedItemChanged = false;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject!.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
            base.OnDetaching();
        }

        #endregion
    }
}
