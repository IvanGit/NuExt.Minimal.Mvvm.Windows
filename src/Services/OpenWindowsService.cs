using System.Diagnostics;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides services for managing open window view models within the application.
    /// This service maintains a list of currently open window view models and offers functionality to register,
    /// unregister, and force-close all registered windows asynchronously. It ensures thread safety using an asynchronous lock.
    /// </summary>
    public sealed class OpenWindowsService : ServiceBase<FrameworkElement>, IOpenWindowsService
    {
        private readonly List<WindowViewModel> _viewModels = [];
        private readonly AsyncLock _lock = new();

        /// <summary>
        /// Gets open window view models.
        /// </summary>
        public IEnumerable<WindowViewModel> ViewModels => _viewModels;

        /// <summary>
        /// Asynchronously disposes the service, force-closing all registered windows.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_lock.IsDisposed)
            {
                return;
            }
            await _lock.EnterAsync();
            try
            {
                List<Exception>? exceptions = null;
                for (int i = _viewModels.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        await _viewModels[i].CloseAsync(force: true);
                    }
                    catch (Exception ex)
                    {
                        exceptions ??= [];
                        exceptions.Add(ex);
                    }
                }
                if (exceptions is not null)
                {
                    throw new AggregateException(exceptions);
                }
                //Debug.Assert(_viewModels.Count == 0);
                _viewModels.Clear();
            }
            finally
            {
                _lock.Exit();
            }
            _lock.Dispose();
        }

        /// <summary>
        /// Registers a window view model with the service.
        /// </summary>
        /// <param name="viewModel">The window view model to register.</param>
        public void Register(WindowViewModel viewModel)
        {
            _lock.Enter();
            try
            {
                _viewModels.Add(viewModel);
            }
            finally
            {
                _lock.Exit();
            }
        }

        /// <summary>
        /// Unregisters a window view model from the service.
        /// </summary>
        /// <param name="viewModel">The window view model to unregister.</param>
        public void Unregister(WindowViewModel viewModel)
        {
            if (_lock.IsEntered)//is disposing
            {
                return;
            }
            _lock.Enter();
            try
            {
                bool flag = _viewModels.Remove(viewModel);
                Debug.Assert(flag);
            }
            finally
            {
                _lock.Exit();
            }
        }
    }
}
