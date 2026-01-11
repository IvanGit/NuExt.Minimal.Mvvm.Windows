using System;
using System.Threading.Tasks;
using System.Windows;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides services for invoking delegates on the UI thread dispatcher.
    /// </summary>
    /// <remarks>
    /// This service is typically attached to <see cref="FrameworkElement"/> instances and
    /// facilitates thread-safe interaction with UI elements by marshaling calls to the UI thread.
    /// </remarks>
    public class DispatcherService : ServiceBase<FrameworkElement>, IDispatcherService
    {
        /// <inheritdoc/>
        public Task<object?> BeginInvoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            return Dispatcher.InvokeAsync(() => method.Call(args)).Task;
        }

        /// <inheritdoc/>
        public object? Invoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (!Dispatcher.CheckAccess())
            {
                return Dispatcher.Invoke(method, args);
            }

            return method.Call(args);
        }

        /// <inheritdoc/>
        public void Invoke(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(callback);
                return;
            }

            callback();
        }

        /// <inheritdoc/>
        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (!Dispatcher.CheckAccess())
            {
                return Dispatcher.Invoke(callback);
            }

            return callback();
        }

        /// <inheritdoc/>
        public Task InvokeAsync(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task;
        }

        /// <inheritdoc/>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(Func<Task> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task.Unwrap();
        }

        /// <inheritdoc/>
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task.Unwrap();
        }
    }
}
