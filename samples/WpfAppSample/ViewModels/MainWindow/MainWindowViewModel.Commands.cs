using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using System.Diagnostics;
using System.Windows.Input;
using WpfAppSample.Views;
using static AccessModifier;

namespace WpfAppSample.ViewModels
{
    partial class MainWindowViewModel
    {
        #region Commands

        [Notify(Setter = Private)]
        private ICommand? _activeDocumentChangedCommand;

        [Notify(Setter = Private)]
        private ICommand? _closeActiveDocumentCommand;

        [Notify(Setter = Private)]
        private ICommand? _showHideActiveDocumentCommand;

        [Notify(Setter = Private)]
        private ICommand? _showMoviesCommand;

        #endregion

        #region Command Methods

        private bool CanCloseActiveDocument()
        {
            return IsUsable && ActiveDocument != null;
        }

        private async Task CloseActiveDocumentAsync()
        {
            await ActiveDocument!.CloseAsync();
        }

        private bool CanShowHideActiveDocument(bool show)
        {
            return IsUsable && ActiveDocument != null;
        }

        private void ShowHideActiveDocument(bool show)
        {
            if (show)
            {
                ActiveDocument?.Show();
            }
            else
            {
                ActiveDocument?.Hide();
            }
        }

        private bool CanShowMovies()
        {
            return IsUsable && DocumentManagerService != null;
        }

        private async Task ShowMoviesAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            var document = await DocumentManagerService!.FindDocumentByIdOrCreateAsync(default(Movies),
                async x =>
                {
                    var vm = new MoviesViewModel() { Title = "Movies" };
                    try
                    {
                        var doc = await x.CreateDocumentAsync(nameof(MoviesView), vm, this, null, cancellationToken);
                        doc.DisposeOnClose = true;
                        return doc;
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(ex is OperationCanceledException, ex.Message);
                        await vm.DisposeAsync();
                        throw;
                    }
                });
            document.Show();
        }

        #endregion

        #region Methods

        protected override void CreateCommands()
        {
            base.CreateCommands();
            ActiveDocumentChangedCommand = RegisterCommand(UpdateTitle);
            ShowMoviesCommand = RegisterAsyncCommand(ShowMoviesAsync, CanShowMovies);
            ShowHideActiveDocumentCommand = RegisterCommand<bool>(ShowHideActiveDocument, CanShowHideActiveDocument);
            CloseActiveDocumentCommand = RegisterAsyncCommand(CloseActiveDocumentAsync, CanCloseActiveDocument);
        }

        protected override async ValueTask OnContentRenderedAsync(CancellationToken cancellationToken)
        {
            await base.OnContentRenderedAsync(cancellationToken);
            Debug.Assert(DocumentManagerService != null, $"{nameof(DocumentManagerService)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");

            Debug.Assert(CheckAccess());
            cancellationToken.ThrowIfCancellationRequested();

            if (DocumentManagerService is IAsyncDisposable asyncDisposable)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable);
            }

            await MoviesService.InitializeAsync(cancellationToken);
            RaiseCanExecuteChanged();

            Debug.Assert(Settings!.IsSuspended == false);
            if (Settings.MoviesOpened)
            {
                ShowMoviesCommand?.Execute(null);
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            CreateSettings();
            UpdateTitle();
        }

        #endregion
    }
}
