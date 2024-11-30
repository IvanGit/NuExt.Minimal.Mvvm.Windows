using Minimal.Mvvm;
using System.Windows;
using WpfAppSample.Models;
using WpfAppSample.Views;
using static AccessModifier;

namespace WpfAppSample.ViewModels
{
    partial class MoviesViewModel
    {
        #region Command Methods

        private bool CanDelete() => CanEdit() && ParentViewModel?.CloseMovieCommand is not null;

        [Notify(Setter = Private)]
        private async Task DeleteAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            var dialogResult = MessageBox.Show(string.Format(Loc.Are_you_sure_you_want_to_delete__Arg0__, SelectedItem?.Name), Loc.Confirmation,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialogResult != MessageBoxResult.Yes)
            {
                return;
            }

            var itemToDelete = SelectedItem!;
            var parentPath = itemToDelete.Parent?.GetPath();
            bool result = await MoviesService.DeleteAsync(itemToDelete, cancellationToken);
            if (result)
            {
                if (itemToDelete is MovieModel movie)
                {
                    await ParentViewModel!.CloseMovieCommand!.ExecuteAsync(movie, cancellationToken);
                }
                await ReloadMoviesAsync(cancellationToken);
                var item = Movies!.FindByPath(parentPath);
                //item?.Expand();
                SelectedItem = item;
            }
        }

        private bool CanEdit()
        {
            return IsUsable && SelectedItem?.IsEditable == true && DialogService != null;
        }

        [Notify(Setter = Private)]
        private async Task EditAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            var clone = SelectedItem!.Clone();

            switch (clone)
            {
                case MovieGroupModel group:
                    await using (var viewModel = new InputDialogViewModel())
                    {
                        viewModel.InputMessage = Loc.Enter_new_group_name;
                        var dlgResult = await DialogService!.ShowDialogAsync(MessageBoxButton.OKCancel,
                            Loc.Edit_Group, nameof(InputDialogView), viewModel, this, group.Name, cancellationToken);
                        if (dlgResult != MessageBoxResult.OK || string.IsNullOrWhiteSpace(viewModel.InputText))
                        {
                            return;
                        }
                        group.Name = viewModel.InputText!;
                    }
                    break;
                case MovieModel movie:

                    await using (var viewModel = new EditMovieViewModel())
                    {
                        var dlgResult = await DialogService!.ShowDialogAsync(MessageBoxButton.OKCancel, Loc.Edit_Movie,
                            nameof(EditMovieView), viewModel, this, movie, cancellationToken);
                        if (dlgResult != MessageBoxResult.OK)
                        {
                            return;
                        }
                    }
                    break;
            }

            var path = clone.GetPath();
            bool result = await MoviesService.SaveAsync(SelectedItem, clone, cancellationToken);
            if (result)
            {
                await ReloadMoviesAsync(cancellationToken);
                var item = Movies!.FindByPath(path);
                //item?.Expand();
                SelectedItem = item;
            }
        }

        private bool CanNewGroup()
        {
            return IsUsable && SelectedItem is MovieGroupModel { IsLost: false } && DialogService != null;
        }

        [Notify(Setter = Private)]
        private async Task NewGroupAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            string? groupName = null;
            await using var viewModel = new InputDialogViewModel();
            viewModel.InputMessage = Loc.Enter_new_group_name;
            var dlgResult = await DialogService!.ShowDialogAsync(MessageBoxButton.OKCancel, Loc.New_Group,
               nameof(InputDialogView), viewModel, this, groupName, cancellationToken);
            if (dlgResult != MessageBoxResult.OK)
            {
                return;
            }
            groupName = viewModel.InputText;

            if (string.IsNullOrWhiteSpace(groupName))
            {
                return;
            }

            var model = new MovieGroupModel()
            {
                Name = groupName!,
                Parent = SelectedItem is MovieGroupModel { IsRoot: false } group ? group : null
            };
            var path = model.GetPath();

            bool result = await MoviesService.AddAsync(model, cancellationToken);
            if (result)
            {
                await ReloadMoviesAsync(cancellationToken);
                var item = Movies!.FindByPath(path);
                //item?.Expand();
                SelectedItem = item;
            }
        }

        private bool CanNewMovie() => CanNewGroup();

        [Notify(Setter = Private)]
        private async Task NewMovieAsync()
        {
            var cancellationToken = GetCurrentCancellationToken();

            await using var viewModel = new EditMovieViewModel();

            var movie = new MovieModel()
            {
                Name = Loc.New_Movie,
                ReleaseDate = DateTime.Today,
                Parent = SelectedItem is MovieGroupModel { IsRoot: false } group ? group : null
            };

            var dlgResult = await DialogService!.ShowDialogAsync(MessageBoxButton.OKCancel, Loc.New_Movie, nameof(EditMovieView), viewModel, this, movie, cancellationToken);
            if (dlgResult != MessageBoxResult.OK)
            {
                return;
            }

            var path = viewModel.Movie.GetPath();
            bool result = await MoviesService.AddAsync(viewModel.Movie, cancellationToken);
            if (result)
            {
                await ReloadMoviesAsync(cancellationToken);
                var item = Movies!.FindByPath(path);
                //item?.Expand();
                SelectedItem = item;
            }
        }

        private bool CanOpenMovie(MovieModelBase? item)
        {
            return IsUsable && item is MovieModel && ParentViewModel?.OpenMovieCommand is not null;
        }

        [Notify(Setter = Private)]
        private async Task OpenMovieAsync(MovieModelBase? item)
        {
            var cancellationToken = GetCurrentCancellationToken();
            await ParentViewModel!.OpenMovieCommand!.ExecuteAsync((item as MovieModel)!, cancellationToken);
        }

        private bool CanMove(MovieModelBase? draggedObject)
        {
            return IsUsable && draggedObject?.CanDrag == true;
        }

        [Notify(Setter = Private)]
        private async Task MoveAsync(MovieModelBase? draggedObject)
        {
            var cancellationToken = GetCurrentCancellationToken();

            var path = draggedObject!.GetPath();
            await ReloadMoviesAsync(cancellationToken);
            var item = Movies!.FindByPath(path);
            //item?.Expand();
            SelectedItem = item;
        }

        [Notify(Setter = Private)]
        private void ExpandOrCollapse(bool expand)
        {
            if (expand)
            {
                Movies!.OfType<MovieGroupModel>().ForEach(m => m.ExpandAll());
            }
            else
            {
                Movies!.OfType<MovieGroupModel>().ForEach(m => m.CollapseAll());
            }
        }

        #endregion

        #region Methods

        protected override void CreateCommands()
        {
            base.CreateCommands();
            DeleteCommand = RegisterAsyncCommand(DeleteAsync, CanDelete);
            EditCommand = RegisterAsyncCommand(EditAsync, CanEdit);
            NewGroupCommand = RegisterAsyncCommand(NewGroupAsync, CanNewGroup);
            NewMovieCommand = RegisterAsyncCommand(NewMovieAsync, CanNewMovie);
            MoveCommand = RegisterAsyncCommand<MovieModelBase?>(MoveAsync, CanMove);
            OpenMovieCommand = RegisterAsyncCommand<MovieModelBase?>(OpenMovieAsync, CanOpenMovie);
            ExpandOrCollapseCommand = RegisterCommand<bool>(ExpandOrCollapse, _ => IsUsable);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            CreateSettings();
            if (!string.IsNullOrEmpty(Settings!.SelectedPath))
            {
                SelectedItem = Movies?.FindByPath(Settings.SelectedPath);
            }
        }

        #endregion
    }
}
