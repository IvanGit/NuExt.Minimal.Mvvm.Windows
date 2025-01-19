using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace MovieWpfApp.ViewModels
{
    internal sealed partial class MainWindowViewModel : WindowViewModel
    {
        private IAsyncDocument? _lastActiveDocument;
        private IAsyncDocument? _lastActiveWindow;

        #region Properties

        [Notify(CallbackName = nameof(OnActiveDocumentChanged))]
        private IAsyncDocument? _activeDocument;

        [Notify(CallbackName = nameof(OnActiveWindowChanged))]
        private IAsyncDocument? _activeWindow;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = [];

        #endregion

        #region Services

        public IAsyncDocumentManagerService? DocumentManagerService => GetService<IAsyncDocumentManagerService>("Documents");

        public EnvironmentService EnvironmentService => GetService<EnvironmentService>()!;

        private MoviesService MoviesService => GetService<MoviesService>()!;

        private SettingsService? SettingsService => GetService<SettingsService>();

        public IAsyncDocumentManagerService? WindowManagerService => GetService<IAsyncDocumentManagerService>("Windows");

        #endregion

        #region Event Handlers

        private void DocumentManagerService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IAsyncDocumentManagerService documentManagerService) return;
            if (e.PropertyName == nameof(documentManagerService.Count))
            {
                if (documentManagerService.Count == 0)
                {
                    _lastActiveDocument = null;
                }
            }
        }

        private void OnActiveDocumentChanged(IAsyncDocument? oldActiveDocument)
        {
            ShowHideActiveDocumentCommand?.RaiseCanExecuteChanged();
            CloseActiveDocumentCommand?.RaiseCanExecuteChanged();

            if (ActiveDocument != null)
            {
                _lastActiveDocument = ActiveDocument;
            }
        }

        private void OnActiveWindowChanged(IAsyncDocument? oldActiveWindow)
        {
            ShowHideActiveWindowCommand?.RaiseCanExecuteChanged();
            CloseActiveWindowCommand?.RaiseCanExecuteChanged();

            if (ActiveWindow != null)
            {
                _lastActiveWindow = ActiveWindow;
            }
        }

        private void WindowManagerService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IAsyncDocumentManagerService documentManagerService) return;
            if (e.PropertyName == nameof(documentManagerService.Count))
            {
                if (documentManagerService.Count == 0)
                {
                    _lastActiveWindow = null;
                }
            }
        }

        #endregion

        #region Methods

        protected override ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            MessageBoxResult result = MessageBox.Show(
                string.Format(Loc.Are_you_sure_you_want_to_close__Arg0__, $"{AssemblyInfo.Product}"),
                Loc.Confirmation,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return new ValueTask<bool>(false);
            }

            return base.CanCloseAsync(cancellationToken);
        }

        private ValueTask LoadMenuAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MenuItems.Clear();
            var menuItems = new MenuItemViewModel[]
            {
                new()
                {
                    Header = Loc.File,
                    SubMenuItems=new ObservableCollection<MenuItemViewModel?>(new MenuItemViewModel?[]
                    {
                        new() { Header = Loc.Movies, Command = ShowMoviesCommand },
                        null,
                        new() { Header = Loc.Exit, Command = CloseCommand }
                    })
                },
                new()
                {
                    Header = Loc.View,
                    SubMenuItems=new ObservableCollection<MenuItemViewModel?>(new MenuItemViewModel?[]
                    {
                        new() { Header = Loc.Hide_Active_Document, CommandParameter = false, Command = ShowHideActiveDocumentCommand },
                        new() { Header = Loc.Show_Active_Document, CommandParameter = true, Command = ShowHideActiveDocumentCommand },
                        new() { Header = Loc.Close_Active_Document, Command = CloseActiveDocumentCommand },
                        null,
                        new() { Header = Loc.Hide_Active_Window, CommandParameter = false, Command = ShowHideActiveWindowCommand },
                        new() { Header = Loc.Show_Active_Window, CommandParameter = true, Command = ShowHideActiveWindowCommand },
                        new() { Header = Loc.Close_Active_Window, Command = CloseActiveWindowCommand },
                    })
                }
            };
            menuItems.ForEach(MenuItems.Add);
            return default;
        }

        protected override async ValueTask OnDisposeAsync()
        {
            var doc = DocumentManagerService?.FindDocumentById(default(Movies));
            Settings!.MoviesOpened = doc is not null;

            await base.OnDisposeAsync();
        }

        protected override void OnError(Exception ex, [CallerMemberName] string? callerName = null)
        {
            base.OnError(ex, callerName);
            MessageBox.Show(string.Format(Loc.An_error_has_occurred_in_Arg0_Arg1, callerName, ex.Message), Loc.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(DocumentManagerService != null, $"{nameof(DocumentManagerService)} is null");
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");
            Debug.Assert(WindowManagerService != null, $"{nameof(WindowManagerService)} is null");

            if (DocumentManagerService is IAsyncDisposable asyncDisposable)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable);
            }
            if (DocumentManagerService is INotifyPropertyChanged notifyPropertyChanged)
            {
                Lifetime.AddBracket(
                    () => notifyPropertyChanged.PropertyChanged += DocumentManagerService_PropertyChanged,
                    () => notifyPropertyChanged.PropertyChanged -= DocumentManagerService_PropertyChanged);
            }

            if (WindowManagerService is IAsyncDisposable asyncDisposable1)
            {
                Lifetime.AddAsyncDisposable(asyncDisposable1);
            }
            if (WindowManagerService is INotifyPropertyChanged notifyPropertyChanged1)
            {
                Lifetime.AddBracket(
                    () => notifyPropertyChanged1.PropertyChanged += WindowManagerService_PropertyChanged,
                    () => notifyPropertyChanged1.PropertyChanged -= WindowManagerService_PropertyChanged);
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        private void UpdateTitle()
        {
            var sb = new ValueStringBuilder(stackalloc char[200]);
            var doc = ActiveDocument;
            if (doc != null)
            {
                sb.Append($"{doc.Title} - ");
            }
            sb.Append($"{AssemblyInfo.Product} v{AssemblyInfo.Version?.ToString(3)}");
            var window = ActiveWindow;
            if (window != null)
            {
                sb.Append($" [{window.Title}]");
            }
            Title = sb.ToString();
        }

        #endregion
    }
}
