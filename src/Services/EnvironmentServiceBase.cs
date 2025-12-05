using System.IO;
using System.Reflection;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a base implementation for environment-related services, initializing 
    /// various directories such as the base directory, configuration directory, application data directory, 
    /// working directory, logs directory and settings directory.
    /// </summary>
    public abstract class EnvironmentServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentServiceBase"/> class with the specified base directory and arguments.
        /// </summary>
        /// <param name="baseDirectory">The base directory of the application.</param>
        /// <param name="appDataDirectory">The application data directory.</param>
        /// <param name="args">An array of arguments passed to the application.</param>
        protected EnvironmentServiceBase(string baseDirectory, string appDataDirectory, params string[] args)
        {
            BaseDirectory = baseDirectory;
            AppDataDirectory = appDataDirectory;
            Args = args;

            ConfigDirectory = Path.Combine(BaseDirectory, "Config");
            IOUtils.CheckDirectory(ConfigDirectory, true);

            IOUtils.CheckDirectory(AppDataDirectory, true);

            WorkingDirectory = Path.Combine(AppDataDirectory, AssemblyInfo.Current.Version!.ToString(2));
            IOUtils.CheckDirectory(WorkingDirectory, true);

            LogsDirectory = Path.Combine(WorkingDirectory, "Logs");
            IOUtils.CheckDirectory(LogsDirectory, true);

            SettingsDirectory = Path.Combine(WorkingDirectory, "Settings");
            IOUtils.CheckDirectory(SettingsDirectory, true);
        }

        #region Properties

        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        public string AppDataDirectory { get; }

        /// <summary>
        /// Gets the arguments passed to the application.
        /// </summary>
        public string[] Args { get; }

        /// <summary>
        /// Gets the application configuration directory.
        /// </summary>
        public string ConfigDirectory { get; }

        /// <summary>
        /// Gets the base application directory.
        /// </summary>
        public string BaseDirectory { get; }

        /// <summary>
        /// Gets the logs directory where application logs are stored.
        /// </summary>
        public string LogsDirectory { get; }

        /// <summary>
        /// Gets the application settings directory.
        /// </summary>
        public string SettingsDirectory { get; }

        /// <summary>
        /// Gets the working directory of the application, typically version-specific.
        /// </summary>
        public string WorkingDirectory { get; }

        #endregion
    }
}
