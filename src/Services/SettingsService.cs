﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a service for saving and loading settings.
    /// </summary>
    public class SettingsService : ServiceBase<FrameworkElement>, ISettingsService
    {
        public static readonly DependencyProperty DirectoryNameProperty =
            DependencyProperty.Register(nameof(DirectoryName), typeof(string), typeof(SettingsService), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(SettingsService), new PropertyMetadata(string.Empty));

        #region Properties

        /// <summary>
        /// Gets or sets the directory name where settings files are stored.
        /// </summary>
        public string DirectoryName
        {
            get => (string)GetValue(DirectoryNameProperty);
            set => SetValue(DirectoryNameProperty, value);
        }

        /// <summary>
        /// Gets or sets the prefix used for settings files.
        /// </summary>
        public string? Prefix
        {
            get => (string)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an error is encountered during load or save operations.
        /// </summary>
        public event ErrorEventHandler? Error;

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            if (string.IsNullOrWhiteSpace(Prefix))
            {
                Prefix = AssociatedObject!.GetType().Name;
            }
        }

        /// <summary>
        /// Constructs the filename for the settings file based on the given name and the current prefix.
        /// </summary>
        /// <param name="name">The base name of the settings file.</param>
        /// <returns>The constructed filename.</returns>
        private string GetFileName(string name)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(name);
#else
            Throw.IfNullOrEmpty(name);
#endif
            var sb = new ValueStringBuilder(260);
            if (!string.IsNullOrEmpty(Prefix))
            {
                sb.Append(Prefix);
#if NET8_0_OR_GREATER
                if (!Prefix.EndsWith('.'))
#else
                if (!Prefix!.EndsWith("."))
#endif
                {
                    sb.Append('.');
                }
            }
            sb.Append(name);
            string fileName = sb.ToString();
            if (Path.HasExtension(name)) return IOUtils.ClearFileName(fileName)!;
            return IOUtils.ClearFileName(fileName) + ".json";
        }

        /// <summary>
        /// Loads the settings from a file into the specified bindable object.
        /// </summary>
        /// <param name="settings">The bindable object to load settings into.</param>
        /// <param name="name">The name of the settings file (default is "Settings").</param>
        /// <returns>true if the settings were loaded successfully; otherwise, false.</returns>
        public bool LoadSettings(SettingsBase settings, string name = "Settings")
        {
            Debug.Assert(settings != null, "settings is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(settings);
#else
            Throw.IfNull(settings);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(name);
#else
            Throw.IfNullOrEmpty(name);
#endif

            try
            {
                string filePath = Path.Combine(DirectoryName, GetFileName(name));
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    if (!string.IsNullOrEmpty(json))
                    {
                        ObjectUtils.DeserializeObject(json, settings.GetPropertyType, settings.SetProperty);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                Error?.Invoke(this, new ErrorEventArgs(ex));
            }
            return false;
        }

        /// <summary>
        /// Saves the settings from the specified bindable object to a file.
        /// </summary>
        /// <param name="settings">The bindable object containing the settings to save.</param>
        /// <param name="name">The name of the settings file (default is "Settings").</param>
        /// <returns>true if the settings were saved successfully; otherwise, false.</returns>
        public bool SaveSettings(SettingsBase settings, string name = "Settings")
        {
            Debug.Assert(settings != null, "settings is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(settings);
#else
            Throw.IfNull(settings);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(name);
#else
            Throw.IfNullOrEmpty(name);
#endif

            try
            {
                string filePath = Path.Combine(DirectoryName, GetFileName(name));
                string? s = ObjectUtils.SerializeObject(settings);
                if (!string.IsNullOrEmpty(s))
                {
                    File.WriteAllText(filePath, s);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
                Error?.Invoke(this, new ErrorEventArgs(ex));
            }
            return false;
        }

        #endregion
    }
}
