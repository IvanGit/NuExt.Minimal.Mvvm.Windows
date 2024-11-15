using Minimal.Mvvm;

namespace WpfAppSample.Models
{
    public sealed partial class MoviesSettings : SettingsBase
    {
        [Notify]
        private string? _selectedPath;
    }
}
