using Minimal.Mvvm.Windows;

namespace WpfAppSample.ViewModels
{
    internal class InputDialogViewModel : ControlViewModel
    {
        #region Properties

        private string? _inputMessage;
        public string? InputMessage
        {
            get => _inputMessage;
            set => SetProperty(ref _inputMessage, value);
        }

        private string? _inputText;
        public string? InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        #endregion

        #region Methods

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            if (Parameter is string text)
            {
                InputText = text;
            }
        }

        #endregion
    }
}
