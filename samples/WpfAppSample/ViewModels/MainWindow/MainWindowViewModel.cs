﻿using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using WpfAppSample.Models;
using WpfAppSample.Services;
using WpfAppSample.Views;

namespace WpfAppSample.ViewModels
{
    internal sealed partial class MainWindowViewModel : WindowViewModel
    {
        #region Properties

        private IAsyncDocument? _activeDocument;
        public IAsyncDocument? ActiveDocument
        {
            get => _activeDocument;
            set => SetProperty(ref _activeDocument, value, OnActiveDocumentChanged);
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new();

        #endregion

        #region Services

        public IAsyncDocumentManagerService? DocumentManagerService => GetService<IAsyncDocumentManagerService>("Documents");

        public EnvironmentService EnvironmentService => GetService<EnvironmentService>()!;

        private MoviesService MoviesService => GetService<MoviesService>()!;

        private SettingsService? SettingsService => GetService<SettingsService>();

        #endregion

        #region Event Handlers

        private void OnActiveDocumentChanged(IAsyncDocument? oldActiveDocument)
        {
            ShowHideActiveDocumentCommand?.RaiseCanExecuteChanged();
            CloseActiveDocumentCommand?.RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        public async ValueTask CloseMovieAsync(MovieModel movie, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var doc = DocumentManagerService!.FindDocumentById(new MovieDocument(movie));
            if (doc == null) return;
            await doc.CloseAsync();
        }

        private ValueTask LoadMenuAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MenuItems.Clear();
            var menuItems = new MenuItemViewModel[]
            {
                new()
                {
                    Header = "File",
                    SubMenuItems=new ObservableCollection<MenuItemViewModel?>(new MenuItemViewModel?[]
                    {
                        new() { Header = "Movies", Command = ShowMoviesCommand },
                        null,
                        new() { Header = "Exit", Command = CloseCommand }
                    })
                },
                new()
                {
                    Header = "View",
                    SubMenuItems=new ObservableCollection<MenuItemViewModel?>(new MenuItemViewModel?[]
                    {
                        new() { Header = "Hide Active Document", CommandParameter = false, Command = ShowHideActiveDocumentCommand },
                        new() { Header = "Show Active Document", CommandParameter = true, Command = ShowHideActiveDocumentCommand },
                        new() { Header = "Close Active Document", Command = CloseActiveDocumentCommand }
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
            MessageBox.Show($"An error has occurred in {callerName}:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(EnvironmentService != null, $"{nameof(EnvironmentService)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");

            await base.OnInitializeAsync(cancellationToken);
            await LoadMenuAsync(cancellationToken);
        }

        public async ValueTask OpenMovieAsync(MovieModel movie, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = await DocumentManagerService!.FindDocumentByIdOrCreateAsync(new MovieDocument(movie), async x =>
            {
                var vm = new MovieViewModel();
                try
                {
                    var doc = await x.CreateDocumentAsync(nameof(MovieView), vm, this, movie, cancellationToken);
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

        private void UpdateTitle()
        {
            var sb = new ValueStringBuilder();
            var doc = ActiveDocument;
            if (doc != null)
            {
                sb.Append($"{doc.Title} - ");
            }
            sb.Append($"{AssemblyInfo.Product} v{AssemblyInfo.Version?.ToString(3)}");
            Title = sb.ToString();
        }

        #endregion
    }
}
