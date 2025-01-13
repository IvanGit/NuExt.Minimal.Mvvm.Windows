namespace System.Windows
{
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Asynchronously waits for the specified <see cref="FrameworkElement"/> to be loaded.
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to wait for.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the wait operation.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="element"/> is null.
        /// </exception>
        /// <remarks>
        /// This method ensures that the calling thread has access to the <paramref name="element"/> dispatcher
        /// and waits asynchronously until the element's <see cref="FrameworkElement.Loaded"/> event is raised.
        /// If the element is already loaded, the method will return immediately. If the provided 
        /// <paramref name="cancellationToken"/> is canceled before the element is loaded, the task will be canceled.
        /// </remarks>
        public static async Task WaitLoadedAsync(this FrameworkElement element, CancellationToken cancellationToken)
        {
#if NET
            ArgumentNullException.ThrowIfNull(element);
#else
            Throw.IfNull(element);
#endif
            element.Dispatcher.VerifyAccess();

            if (element.IsLoaded)
            {
                return;
            }

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnLoaded(object? sender, RoutedEventArgs e)
            {
                tcs.TrySetResult(true);
            }

            element.Loaded += OnLoaded;
            try
            {
                using (cancellationToken.CanBeCanceled
                           ? cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false)
                           : null as IDisposable)
                {
                    await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                element.Loaded -= OnLoaded;
            }
        }
    }
}
