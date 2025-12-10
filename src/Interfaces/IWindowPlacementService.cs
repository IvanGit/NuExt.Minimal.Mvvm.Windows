using System;
using System.IO;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Interface providing methods for managing window placement in the application.
    /// Handles saving and restoring the window's position, size, and state.
    /// It also includes events for error handling, and notifications when the window placement is saved or restored.
    /// </summary>
    public interface IWindowPlacementService
    {
        /// <summary>
        /// Occurs when an error happens during the save or restore process.
        /// </summary>
        event ErrorEventHandler? Error;
        /// <summary>
        /// Occurs when the window's placement has been successfully restored.
        /// </summary>
        event EventHandler? Restored;
        /// <summary>
        /// Occurs when the window's placement has been successfully saved.
        /// </summary>
        event EventHandler? Saved;

        /// <summary>
        /// Saves the current placement of the window, including its position, size, and state.
        /// </summary>
        void SavePlacement();
    }
}
