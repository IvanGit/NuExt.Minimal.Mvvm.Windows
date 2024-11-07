namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Defines an interface for an asynchronous document.
    /// </summary>
    public interface IAsyncDocument : IAsyncDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the view model should be disposed when the view is closed.
        /// </summary>
        bool DisposeOnClose { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the document.
        /// </summary>
        object Id { get; set; }
        /// <summary>
        /// Gets or sets the title of the document.
        /// This ID uniquely identifies the document within the document manager.
        /// </summary>
        string? Title { get; set; }

        /// <summary>
        /// Closes the document asynchronously.
        /// </summary>
        /// <param name="force">If set to <c>true</c>, forces the document to close. Default is <c>true</c>.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        ValueTask CloseAsync(bool force = true);
        /// <summary>
        /// Hides the document.
        /// </summary>
        void Hide();
        /// <summary>
        /// Shows the document.
        /// </summary>
        void Show();
    }
}
