using Minimal.Mvvm;

namespace WpfAppSample.Models
{
    public sealed class MoviesSettings : SettingsBase
    {
        private string? _selectedPath;
        public string? SelectedPath
        {
            get => _selectedPath;
            set => SetProperty(ref _selectedPath, value);
        }
    }
}
