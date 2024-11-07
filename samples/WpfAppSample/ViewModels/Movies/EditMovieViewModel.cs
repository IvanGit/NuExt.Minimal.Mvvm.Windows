using Minimal.Mvvm.Windows;
using System.Diagnostics;
using WpfAppSample.Models;
using WpfAppSample.Services;

namespace WpfAppSample.ViewModels
{
    internal sealed class EditMovieViewModel : ControlViewModel
    {
        #region Properties

        public MovieModel Movie => (MovieModel)Parameter!;

        #endregion

        #region Services

        public MoviesService MoviesService => GetService<MoviesService>()!;

        #endregion

        #region Methods

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(Movie != null, $"{nameof(Movie)} is null");
            Debug.Assert(MoviesService != null, $"{nameof(MoviesService)} is null");
            return base.OnInitializeAsync(cancellationToken);
        }

        #endregion
    }
}
