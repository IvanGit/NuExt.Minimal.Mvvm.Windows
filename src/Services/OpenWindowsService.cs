using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides services for managing open window view models within the application.
    /// This service maintains a list of currently open window view models and offers functionality to register,
    /// unregister, and force-close all registered windows asynchronously. It ensures thread safety.
    /// </summary>
    public sealed class OpenWindowsService() : AsyncDisposable(continueOnCapturedContext: true), IOpenWindowsService
    {
        private readonly List<IWindowViewModel> _viewModels = [];
        private readonly AsyncLock _lock = new();

        /// <summary>
        /// Gets open window view models.
        /// </summary>
        public IEnumerable<IWindowViewModel> ViewModels
        {
            get
            {
                CheckDisposedOrDisposing();

                _lock.Enter();
                try
                {
                    return [.. _viewModels];
                }
                finally
                {
                    _lock.Exit();
                }
            }
        }

        /// <summary>
        /// Registers a window view model with the service.
        /// </summary>
        /// <param name="viewModel">The window view model to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the service is disposed.</exception>
        public void Register(IWindowViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            CheckDisposedOrDisposing();

            _lock.Enter();
            try
            {
                CheckDisposedOrDisposing();
                if (!_viewModels.Contains(viewModel))
                {
                    _viewModels.Add(viewModel);
                }
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the service is disposed.</exception>
        public void Unregister(IWindowViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            CheckDisposed();

            if (IsDisposing)//to prevent Unregister call in DisposeAsyncCore->CloseAsync
            {
                return;
            }

            _lock.Enter();
            try
            {
                CheckDisposedOrDisposing();

                bool removed = _viewModels.Remove(viewModel);

                if (!removed)
                {
                    string message = $"{nameof(WindowViewModel)} was not found in the registry: {viewModel}";
                    Trace.WriteLine(message);
                    Debug.Fail(message);
                }
            }
            finally
            {
                _lock.Exit();
            }
        }

        /// <summary>
        /// Asynchronously disposes the service, force-closing all registered windows.
        /// </summary>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        /// <exception cref="AggregateException">Thrown when one or more windows fail to close.</exception>
        protected override async ValueTask DisposeAsyncCore()
        {
            Debug.Assert(ContinueOnCapturedContext);
            Debug.Assert(CheckAccess());
            VerifyAccess();

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

    }
}
