using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// A behavior that monitors the visibility of scroll bars in a <see cref="ScrollViewer"/> and updates attached properties accordingly.
    /// </summary>
    public class ScrollBarVisibilityBehavior : EventTriggerBase<Control>
    {
        static ScrollBarVisibilityBehavior()
        {
            EventProperty.OverrideMetadata(typeof(ScrollBarVisibilityBehavior), new PropertyMetadata(ScrollViewer.ScrollChangedEvent));
            EventNameProperty.OverrideMetadata(typeof(ScrollBarVisibilityBehavior), new PropertyMetadata((string?)null));
        }

        public ScrollBarVisibilityBehavior()
        {
            Debug.Assert(Event == ScrollViewer.ScrollChangedEvent);
            Debug.Assert(EventName == null);
        }

        #region Dependency Properties

        #region ComputedHorizontalScrollBarVisibility

        private static readonly DependencyPropertyKey ComputedHorizontalScrollBarVisibilityPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ComputedHorizontalScrollBarVisibility", typeof(Visibility), typeof(ScrollBarVisibilityBehavior), new UIPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ComputedHorizontalScrollBarVisibilityProperty = ComputedHorizontalScrollBarVisibilityPropertyKey.DependencyProperty;

        public static Visibility GetComputedHorizontalScrollBarVisibility(DependencyObject element)
        {
            return (Visibility)element.GetValue(ComputedHorizontalScrollBarVisibilityProperty);
        }

        private static void SetComputedHorizontalScrollBarVisibility(DependencyObject element, Visibility value)
        {
            element.SetValue(ComputedHorizontalScrollBarVisibilityPropertyKey, value);
        }

        #endregion

        #region ComputedVerticalScrollBarVisibility

        private static readonly DependencyPropertyKey ComputedVerticalScrollBarVisibilityPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ComputedVerticalScrollBarVisibility", typeof(Visibility), typeof(ScrollBarVisibilityBehavior), new UIPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ComputedVerticalScrollBarVisibilityProperty = ComputedVerticalScrollBarVisibilityPropertyKey.DependencyProperty;

        public static Visibility GetComputedVerticalScrollBarVisibility(DependencyObject element)
        {
            return (Visibility)element.GetValue(ComputedVerticalScrollBarVisibilityProperty);
        }

        private static void SetComputedVerticalScrollBarVisibility(DependencyObject element, Visibility value)
        {
            element.SetValue(ComputedVerticalScrollBarVisibilityPropertyKey, value);
        }

        #endregion

        #region HorizontalScrollBarActualHeight

        private static readonly DependencyPropertyKey HorizontalScrollBarActualHeightPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("HorizontalScrollBarActualHeight", typeof(double), typeof(ScrollBarVisibilityBehavior), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty HorizontalScrollBarActualHeightProperty = HorizontalScrollBarActualHeightPropertyKey.DependencyProperty;

        public static double GetHorizontalScrollBarActualHeight(DependencyObject element)
        {
            return (double)element.GetValue(HorizontalScrollBarActualHeightProperty);
        }

        private static void SetHorizontalScrollBarActualHeight(DependencyObject element, double value)
        {
            element.SetValue(HorizontalScrollBarActualHeightPropertyKey, value);
        }

        #endregion

        #region HorizontalScrollBarActualWidth

        private static readonly DependencyPropertyKey HorizontalScrollBarActualWidthPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("HorizontalScrollBarActualWidth", typeof(double), typeof(ScrollBarVisibilityBehavior), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty HorizontalScrollBarActualWidthProperty = HorizontalScrollBarActualWidthPropertyKey.DependencyProperty;

        public static double GetHorizontalScrollBarActualWidth(DependencyObject element)
        {
            return (double)element.GetValue(HorizontalScrollBarActualWidthProperty);
        }

        private static void SetHorizontalScrollBarActualWidth(DependencyObject element, double value)
        {
            element.SetValue(HorizontalScrollBarActualWidthPropertyKey, value);
        }

        #endregion

        #region VerticalScrollBarActualHeight

        private static readonly DependencyPropertyKey VerticalScrollBarActualHeightPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("VerticalScrollBarActualHeight", typeof(double), typeof(ScrollBarVisibilityBehavior), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty VerticalScrollBarActualHeightProperty = VerticalScrollBarActualHeightPropertyKey.DependencyProperty;

        public static double GetVerticalScrollBarActualHeight(DependencyObject element)
        {
            return (double)element.GetValue(VerticalScrollBarActualHeightProperty);
        }

        private static void SetVerticalScrollBarActualHeight(DependencyObject element, double value)
        {
            element.SetValue(VerticalScrollBarActualHeightPropertyKey, value);
        }

        #endregion

        #region VerticalScrollBarActualWidth

        private static readonly DependencyPropertyKey VerticalScrollBarActualWidthPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("VerticalScrollBarActualWidth", typeof(double), typeof(ScrollBarVisibilityBehavior), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty VerticalScrollBarActualWidthProperty = VerticalScrollBarActualWidthPropertyKey.DependencyProperty;

        public static double GetVerticalScrollBarActualWidth(DependencyObject element)
        {
            return (double)element.GetValue(VerticalScrollBarActualWidthProperty);
        }

        private static void SetVerticalScrollBarActualWidth(DependencyObject element, double value)
        {
            element.SetValue(VerticalScrollBarActualWidthPropertyKey, value);
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Called when the associated ScrollChangedEvent is raised.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The event data.</param>
        protected override void OnEvent(object? sender, object? eventArgs)
        {
            if (eventArgs is ScrollChangedEventArgs { OriginalSource: ScrollViewer scrollViewer })
            {
                SetAttachedProperties(scrollViewer);
            }
        }

        /// <summary>
        /// Sets the scroll bar visibility and dimensions properties on the associated object.
        /// </summary>
        /// <param name="scrollViewer">The <see cref="ScrollViewer"/> whose properties are being monitored.</param>
        private void SetAttachedProperties(ScrollViewer scrollViewer)
        {
            var element = AssociatedObject;
            if (element == null)
            {
                return;
            }
            SetComputedHorizontalScrollBarVisibility(element, scrollViewer.ComputedHorizontalScrollBarVisibility);
            SetComputedVerticalScrollBarVisibility(element, scrollViewer.ComputedVerticalScrollBarVisibility);

            var horizontalScrollBar = scrollViewer.Template.FindName("PART_HorizontalScrollBar", scrollViewer) as ScrollBar;
            var verticalScrollBar = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as ScrollBar;
            Debug.Assert(horizontalScrollBar != null);
            Debug.Assert(verticalScrollBar != null);

            SetHorizontalScrollBarActualHeight(element, horizontalScrollBar?.ActualHeight ?? 0);
            SetHorizontalScrollBarActualWidth(element, horizontalScrollBar?.ActualWidth ?? 0);

            SetVerticalScrollBarActualHeight(element, verticalScrollBar?.ActualHeight ?? 0);
            SetVerticalScrollBarActualWidth(element, verticalScrollBar?.ActualWidth ?? 0);
        }

        #endregion
    }
}
