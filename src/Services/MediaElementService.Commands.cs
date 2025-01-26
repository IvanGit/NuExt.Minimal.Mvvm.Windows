namespace Minimal.Mvvm.Windows
{
    partial class MediaElementService
    {
        #region Commands

        public RelayCommand PauseCommand { get; } = null!;

        public RelayCommand PlayCommand { get; } = null!;

        public RelayCommand StopCommand { get; } = null!;

        #endregion

        #region Command Methods

        private bool CanPauseInternal()
        {
            return IsPlaying && CanPause;
        }

        private bool CanPlay()
        {
            return !IsPlaying || IsPaused;
        }

        private bool CanStop()
        {
            return IsPlaying || IsPaused;
        }

        #endregion
    }
}
