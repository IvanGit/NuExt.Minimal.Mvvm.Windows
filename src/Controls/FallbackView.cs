using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Minimal.Mvvm.Windows.Controls
{
    public class FallbackView : Panel
    {
        public FallbackView()
        {
            var tb = new TextBlock();
            if (ViewModelBase.IsInDesignMode)
            {
                tb.FontSize = 24;
                tb.Foreground = Brushes.Red;
            }
            else
            {
                tb.FontSize = 20;
                tb.Foreground = Brushes.Gray;
            }
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            Children.Add(tb);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(FallbackView),
            new PropertyMetadata(null, (d, e) => ((FallbackView)d).OnTextChanged()));

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void OnTextChanged()
        {
            ((TextBlock)Children[0]).Text = Text;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ((TextBlock)Children[0]).Measure(availableSize);
            return ((TextBlock)Children[0]).DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ((TextBlock)Children[0]).Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}
