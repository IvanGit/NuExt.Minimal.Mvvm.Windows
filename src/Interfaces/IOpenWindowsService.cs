using System;
using System.Collections.Generic;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Interface providing methods to manage open windows in the application.
    /// Includes methods for registering and unregistering window view models,
    /// as well as a property to retrieve the list of currently registered windows.
    /// </summary>
    public interface IOpenWindowsService : IAsyncDisposable
    {
        /// <summary>
        /// Gets the collection of registered window view models.
        /// </summary>
        IEnumerable<WindowViewModel> ViewModels { get; }

        /// <summary>
        /// Registers a window view model.
        /// </summary>
        /// <param name="viewModel">The view model to register.</param>
        void Register(WindowViewModel viewModel);

        /// <summary>
        /// Unregisters a window view model.
        /// </summary>
        /// <param name="viewModel">The view model to unregister.</param>
        void Unregister(WindowViewModel viewModel);
    }
}
