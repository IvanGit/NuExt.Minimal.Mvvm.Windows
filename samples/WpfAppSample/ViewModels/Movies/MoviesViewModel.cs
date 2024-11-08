﻿using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using WpfAppSample.Models;
using WpfAppSample.Services;

namespace WpfAppSample.ViewModels
{
    internal sealed partial class MoviesViewModel : DocumentContentViewModelBase
    {
        #region Properties

        private ObservableCollection<MovieModelBase>? _movies;
        public ObservableCollection<MovieModelBase>? Movies
        {
            get => _movies;
            private set => SetProperty(ref _movies, value);
        }

        private ListCollectionView? _moviesView;
        public ListCollectionView? MoviesView
        {
            get => _moviesView;
            private set => SetProperty(ref _moviesView, value);
        }

        private MovieModelBase? _selectedItem;
        public MovieModelBase? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value, OnSelectedItemChanged);
        }

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
            base.OnError(ex, callerName);
            MessageBox.Show($"An error has occurred in {callerName}:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(DialogService != null, $"{nameof(DialogService)} is null");
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            Debug.Assert(ParentViewModel != null, $"{nameof(ParentViewModel)} is null");
            Debug.Assert(SettingsService != null, $"{nameof(SettingsService)} is null");

            await base.OnInitializeAsync(cancellationToken);

            Movies = new ObservableCollection<MovieModelBase>();
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
