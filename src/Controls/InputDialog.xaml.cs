using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows.Controls
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : InputDialogBase
    {
        #region Dependency Properties

        /// <summary>Identifies the <see cref="CommandsSource"/> dependency property.</summary>
        public static readonly DependencyProperty CommandsSourceProperty = DependencyProperty.Register(
            nameof(CommandsSource), typeof(IEnumerable), typeof(InputDialog));

        #endregion

        private UICommand? _tcs;

        public InputDialog()
        {
            InitializeComponent();
        }

        #region UI Commands

        private UICommand? CancelCommand => CommandsSource?.Cast<UICommand>().FirstOrDefault(c => c.IsCancel);

        private UICommand? DefaultCommand => CommandsSource?.Cast<UICommand>().FirstOrDefault(c => c.IsDefault);

        #endregion

        #region Properties

        public IEnumerable? CommandsSource
        {
            get => (IEnumerable)GetValue(CommandsSourceProperty);
            set => SetValue(CommandsSourceProperty, value);
        }

        #endregion

        #region Event Handlers

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button { DataContext: UICommand command })
            {
                _tcs = command;
                Close();
                e.Handled = true;
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e is { Key: Key.System, SystemKey: Key.F4 })
            {
                _tcs = CancelCommand;
                DialogResult = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                var result = DefaultCommand;
                if (e.OriginalSource is Button { DataContext: UICommand command })
                {
                    result = command;
                }

                if (result != null)
                {
                    _tcs = result;
                    DialogResult = true;
                }
                e.Handled = true;
            }
        }

        #endregion

        #region Methods

        private Lifetime SubscribeMetroDialog(CancellationToken cancellationToken)
        {
            var lifetime = new Lifetime();

            if (cancellationToken.CanBeCanceled)
            {
                lifetime.AddDisposable(cancellationToken.Register(Close, useSynchronizationContext: true));
            }

            if (DialogBottom != null && DialogButtons != null)
            {
                foreach (Button button in DialogButtons.FindChildren<Button>())
                {
                    if (button.Command is null && button.DataContext is UICommand command)
                    {
                        lifetime.AddBracket(() => button.Click += OnButtonClick, () => button.Click -= OnButtonClick);
                        lifetime.AddBracket(() => button.KeyDown += OnKeyDownHandler, () => button.KeyDown += OnKeyDownHandler);
                    }
                }
            }

            lifetime.AddBracket(() => KeyDown += OnKeyDownHandler, () => KeyDown -= OnKeyDownHandler);

            return lifetime;
        }

        public UICommand? ShowDialog(CancellationToken cancellationToken)
        {
            Lifetime? subscription = null;
            void OnDialogContentRendered(object? sender, EventArgs e)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                subscription ??= SubscribeMetroDialog(cancellationToken);
            }
            ContentRendered += OnDialogContentRendered;
            try
            {
                var res = ShowDialog();
                cancellationToken.ThrowIfCancellationRequested();
                Debug.Assert(res == DialogResult && res != null);
                if (_tcs == null && res != true)
                {
                    return CancelCommand;
                }
                return _tcs;
            }
            finally
            {
                ContentRendered -= OnDialogContentRendered;
                Disposable.DisposeAndNull(ref subscription);
            }
        }

        #endregion
    }
}
