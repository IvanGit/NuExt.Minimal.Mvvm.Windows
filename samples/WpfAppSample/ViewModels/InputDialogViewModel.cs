using Minimal.Mvvm;
using Minimal.Mvvm.Windows;

namespace WpfAppSample.ViewModels
{
    internal partial class InputDialogViewModel : ControlViewModel
    {
        #region Properties

        [Notify]
        private string? _inputMessage;

        [Notify]
        private string? _inputText;

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
