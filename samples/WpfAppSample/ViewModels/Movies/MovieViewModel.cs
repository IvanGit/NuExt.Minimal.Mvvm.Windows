using Minimal.Mvvm;
using Minimal.Mvvm.Windows;
using MovieWpfApp.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using static AccessModifier;

namespace MovieWpfApp.ViewModels
{
    internal sealed partial class MovieViewModel : DocumentContentViewModelBase
    {
        #region Properties

        [Notify(Setter = Private)]
        private bool _isWindowed;

        public MovieModel Movie => (MovieModel)Parameter!;

        #endregion

        #region Services

        private WindowService? WindowService => GetService<WindowService>();

        #endregion

        #region Event Handlers

        private void Movie_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(MovieModel.Name) or nameof(MovieModel.ReleaseDate))
            {
                UpdateTitle();
            }
        }

        #endregion

        #region Methods

        public override ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            var dialogResult = MessageBox.Show(string.Format(Loc.Are_you_sure_you_want_to_close__Arg0__, Movie.Name), Loc.Confirmation,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialogResult != MessageBoxResult.Yes)
            {
                return new ValueTask<bool>(false);
            }

            return base.CanCloseAsync(cancellationToken);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(Parameter is MovieModel);
            await base.OnInitializeAsync(cancellationToken);
            Lifetime.AddBracket(() => Movie.PropertyChanged += Movie_PropertyChanged,
                () => Movie.PropertyChanged -= Movie_PropertyChanged);
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            Title = $"{Movie.Name} [{Movie.ReleaseDate:yyyy}]";
        }

        #endregion
    }
}
