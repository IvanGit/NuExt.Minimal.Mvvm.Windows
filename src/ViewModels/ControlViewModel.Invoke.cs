using System;
using System.Threading.Tasks;

namespace Minimal.Mvvm.Windows
{
    partial class ControlViewModel
    {
        #region Methods

        /// <summary>
        /// Executes the specified delegate asynchronously on the thread the instance was created on.
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the given method. This can be <see langword="null"/> if no arguments are needed.</param>
        /// <returns>A <see cref="Task{Object}"/> that represents the asynchronous operation. 
        /// The task result is the return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="method"/> parameter is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method returns immediately and does not wait for the asynchronous operation to complete.
        /// The delegate is always executed asynchronously, even if called from the correct thread.
        /// </remarks>
        public Task<object?> BeginInvoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            return Dispatcher.InvokeAsync(() => method.Call(args)).Task;
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the instance was created on.
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the given method. This can be <see langword="null"/> if no arguments are needed.</param>
        /// <returns>The return value from the delegate being invoked, or <see langword="null"/> if the delegate has no return value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="method"/> parameter is <see langword="null"/>.</exception>
        public object? Invoke(Delegate method, params object?[] args)
        {
            ArgumentNullException.ThrowIfNull(method);

            if (!Dispatcher.CheckAccess())
            {
                return Dispatcher.Invoke(method, args);
            }

            return method.Call(args);
        }

        /// <summary>
        /// Executes the specified <see cref="Action"/> synchronously on the thread the instance was created on.
        /// </summary>
        /// <param name="callback">A delegate to invoke through the dispatcher.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
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

        /// <summary>
        /// Executes the specified <see cref="Func{TResult}"/> synchronously on the thread that the instance was created on.
        /// </summary>
        /// <typeparam name="TResult">The type of the callback return value.</typeparam>
        /// <param name="callback">A <see cref="Func{TResult}"/> delegate to invoke through the dispatcher.</param>
        /// <returns>The return value from the delegate being invoked.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            if (!Dispatcher.CheckAccess())
            {
                return Dispatcher.Invoke(callback);
            }

            return callback();
        }

        /// <summary>
        /// Executes the specified <see cref="Action"/> asynchronously on the thread that the instance was created on.
        /// </summary>
        /// <param name="callback">An <see cref="Action"/> delegate to invoke through the dispatcher.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public Task InvokeAsync(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task;
        }

        /// <summary>
        /// Executes the specified <see cref="Func{TResult}"/> asynchronously on the thread that the instance was created on.
        /// </summary>
        /// <typeparam name="TResult">The type of the callback return value.</typeparam>
        /// <param name="callback">A <see cref="Func{TResult}"/> delegate to invoke through the dispatcher.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task;
        }

        /// <summary>
        /// Executes the specified Func&lt;Task&gt; asynchronously on the thread that the instance was created on.
        /// </summary>
        /// <param name="callback">A Func&lt;Task&gt; delegate to invoke through the dispatcher.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public Task InvokeAsync(Func<Task> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task.Unwrap();
        }

        /// <summary>
        /// Executes the specified Func&lt;Task&lt;TResult&gt;&gt; asynchronously on the thread that the instance was created on.
        /// </summary>
        /// <typeparam name="TResult">The type of the callback return value.</typeparam>
        /// <param name="callback">A Func&lt;Task&lt;TResult&gt;&gt; delegate to invoke through the dispatcher.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="callback"/> parameter is <see langword="null"/>.</exception>
        public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return Dispatcher.InvokeAsync(callback).Task.Unwrap();
        }

        #endregion
    }
}
