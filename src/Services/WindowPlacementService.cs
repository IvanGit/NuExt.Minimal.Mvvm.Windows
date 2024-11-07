using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides window placement services, allowing for the restoration and saving of window placement.
    /// This service can be used to persist window size, position, and state between application sessions.
    /// </summary>
    public class WindowPlacementService : WindowServiceBase, IWindowPlacementService
    {
        public static readonly DependencyProperty DirectoryNameProperty = DependencyProperty.Register(
            nameof(DirectoryName), typeof(string), typeof(WindowPlacementService), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            nameof(FileName), typeof(string), typeof(WindowPlacementService), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlacementRestoredCommandProperty = DependencyProperty.Register(
            nameof(PlacementRestoredCommand), typeof(ICommand), typeof(WindowPlacementService));

        public static readonly DependencyProperty PlacementSavedCommandProperty = DependencyProperty.Register(
            nameof(PlacementSavedCommand), typeof(ICommand), typeof(WindowPlacementService));

        #region Properties

        /// <summary>
        /// Gets or sets the directory name where window placement files are stored.
        /// </summary>
        public string DirectoryName
        {
            get => (string)GetValue(DirectoryNameProperty);
            set => SetValue(DirectoryNameProperty, value);
        }

        /// <summary>
        /// Gets or sets the file name used for storing window placement.
        /// </summary>
        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to execute when placement is restored.
        /// </summary>
        public ICommand? PlacementRestoredCommand
        {
            get => (ICommand?)GetValue(PlacementRestoredCommandProperty);
            set => SetValue(PlacementRestoredCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to execute when placement is saved.
        /// </summary>
        public ICommand? PlacementSavedCommand
        {
            get => (ICommand?)GetValue(PlacementSavedCommandProperty);
            set => SetValue(PlacementSavedCommandProperty, value);
        }

        /// <summary>
        /// Gets the full file path for storing window placement data.
        /// </summary>
        private string FilePath => Path.Combine(DirectoryName, GetFileName());

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an error is encountered during load or save operations.
        /// </summary>
        public event ErrorEventHandler? Error;

        /// <summary>
        /// Occurs when window placement has been restored.
        /// </summary>
        public event EventHandler? Restored;

        /// <summary>
        /// Occurs when window placement has been saved.
        /// </summary>
        public event EventHandler? Saved;

        #endregion

        #region Event Handlers

        protected override void OnWindowChanged(Window? oldWindow)
        {
            base.OnWindowChanged(oldWindow);
            if (oldWindow != null)
            {
                oldWindow.Closing -= OnClosing;
                oldWindow.SourceInitialized -= OnSourceInitialized;
            }
            var newWindow = Window;
            if (newWindow != null)
            {
                newWindow.SourceInitialized += OnSourceInitialized;
                newWindow.Closing += OnClosing;
                var source = (HwndSource?)PresentationSource.FromVisual(newWindow);
                if (source != null)
                {
                    OnSourceInitialized(newWindow, EventArgs.Empty);
                }
            }
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            Debug.Assert(Equals(sender, Window), $"{nameof(Window)} is not equal sender");
            var window = sender as Window;
            Debug.Assert(window != null, "Window is null");
            window!.SourceInitialized -= OnSourceInitialized;
            try
            {
                if (File.Exists(FilePath))
                {
                    var s = File.ReadAllText(FilePath);
                    if (window.SetPlacement(s))
                    {
                        OnPlacementRestored(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(ex is OperationCanceledException, ex.Message);
                Error?.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }
            Debug.Assert(Equals(sender, Window), $"{nameof(Window)} is not equal sender");
            var window = sender as Window;
            Debug.Assert(window != null, "Window is null");
            window!.Closing -= OnClosing;
            InternalSavePlacement(window);
        }

        private void OnPlacementRestored(object? sender, EventArgs e)
        {
            Restored?.Invoke(sender, e);
            var placementRestoredCommand = PlacementRestoredCommand;
            if (placementRestoredCommand != null && placementRestoredCommand.CanExecute(e))
            {
                placementRestoredCommand.Execute(e);
            }
        }

        private void OnPlacementSaved(object? sender, EventArgs e)
        {
            Saved?.Invoke(sender, e);
            var placementSavedCommand = PlacementSavedCommand;
            if (placementSavedCommand != null && placementSavedCommand.CanExecute(e))
            {
                placementSavedCommand.Execute(e);
            }
        }

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            if (string.IsNullOrWhiteSpace(FileName))
            {
                FileName = $"{AssociatedObject!.GetType().Name}";
            }
        }

        private string GetFileName()
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(FileName);
#else
            Throw.IfNullOrEmpty(FileName);
#endif
            if (Path.HasExtension(FileName)) return IOUtils.ClearFileName(FileName)!;
            return IOUtils.ClearFileName(FileName) + ".json";
        }

        private void InternalSavePlacement(Window? window)
        {
            try
            {
                var s = window?.GetPlacementAsJson();
                if (!string.IsNullOrEmpty(s))
                {
                    File.WriteAllText(FilePath, s);
                    OnPlacementSaved(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(ex is OperationCanceledException, ex.Message);
                Error?.Invoke(this, new ErrorEventArgs(ex));
            }
        }

        public void SavePlacement()
        {
            InternalSavePlacement(Window);
        }

        #endregion
    }
}
