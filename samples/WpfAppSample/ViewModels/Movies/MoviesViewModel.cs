using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Models;
using MovieWpfApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using static AccessModifier;

namespace MovieWpfApp.ViewModels
{
    internal sealed partial class MoviesViewModel : DocumentContentViewModelBase
    {
        #region Properties

        [Notify(Setter = Private)]
        private ObservableCollection<MovieModelBase>? _movies;

        [Notify(Setter = Private)]
        private ListCollectionView? _moviesView;

        [Notify(CallbackName = nameof(OnSelectedItemChanged))]
        private MovieModelBase? _selectedItem;

        #endregion

        #region Services

        private IAsyncDialogService? DialogService => GetService<IAsyncDialogService>();

        public EnvironmentService EnvironmentService => GetService<EnvironmentService>()!;

        private MoviesService MoviesService => GetService<MoviesService>()!;

        private new MainWindowViewModel? ParentViewModel => base.ParentViewModel as MainWindowViewModel;

        private SettingsService? SettingsService => GetService<SettingsService>();

        #endregion

        #region Event Handlers

        private void OnSelectedItemChanged(MovieModelBase? oldSelectedItem)
        {
            var newSelectedItem = SelectedItem;
            if (newSelectedItem != null)
            {
            }
            NewGroupCommand?.RaiseCanExecuteChanged();
            NewMovieCommand?.RaiseCanExecuteChanged();
            EditCommand?.RaiseCanExecuteChanged();
            DeleteCommand?.RaiseCanExecuteChanged();
            OpenMovieCommand?.RaiseCanExecuteChanged();
            OpenMovieExternalCommand?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        protected override async ValueTask OnDisposeAsync()
        {
            Settings!.SelectedPath = SelectedItem?.GetPath();

            await base.OnDisposeAsync();
        }

        protected override void OnError(Exception ex, [CallerMemberName] string? callerName = null)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<Exception, string?>(OnError), ex, callerName);
                return;
            }

            VerifyAccess();

            MessageBox.Show(GetService<WindowService>()?.Window, string.Format(Loc.An_error_has_occurred_in_Arg0_Arg1, callerName, ex.Message), Loc.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(DialogService != null, $"{nameof(DialogService)} is null");
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            Debug.Assert(ParentViewModel != null, $"{nameof(ParentViewModel)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");

            await base.OnInitializeAsync(cancellationToken);

            Movies = [];
            Lifetime.Add(Movies.Clear);

            MoviesView = new ListCollectionView(Movies);
            Lifetime.Add(MoviesView.DetachFromSourceCollection);

            await ReloadMoviesAsync(cancellationToken);
        }

        private async ValueTask ReloadMoviesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Movies!.Clear();
            var movies = await MoviesService.GetMoviesAsync(cancellationToken);
            movies.ForEach(Movies.Add);
            Movies.OfType<MovieGroupModel>().FirstOrDefault()?.Expand();
        }

        #endregion
    }
}
