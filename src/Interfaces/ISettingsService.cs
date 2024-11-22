using System.IO;
using System.Text.Json;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a service for saving and loading settings.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Occurs when there is an error during the load or save operation.
        /// </summary>
        event ErrorEventHandler? Error;

        /// <summary>
        /// Loads settings into the specified bindable object.
        /// </summary>
        /// <param name="settings">The bindable object into which the settings will be loaded.</param>
        /// <param name="name">The name of the settings file, defaulting to "Settings".</param>
        /// <param name="options">Options to control the behavior during parsing.</param>
        /// <returns>True if the settings were successfully loaded; otherwise, false.</returns>
        bool LoadSettings(SettingsBase settings, string name = "Settings", JsonSerializerOptions? options = null);

        /// <summary>
        /// Saves the settings from the specified bindable object.
        /// </summary>
        /// <param name="settings">The bindable object from which the settings will be saved.</param>
        /// <param name="name">The name of the settings file, defaulting to "Settings".</param>
        /// <param name="options">Options to control the conversion behavior.</param>
        /// <returns>True if the settings were successfully saved; otherwise, false.</returns>
        bool SaveSettings(SettingsBase settings, string name = "Settings", JsonSerializerOptions? options = null);
    }
}
