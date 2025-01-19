namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a base class for view models that represent document content. 
    /// This class extends <see cref="ControlViewModel"/>.
    /// </summary>
    public abstract partial class DocumentContentViewModelBase : ControlViewModel, IAsyncDocumentContent
    {
        #region Properties

        /// <summary>
        /// Gets or sets the title of the document.
        /// </summary>
        [Notify]
        private string? _title;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the document can be closed. Override this method to provide custom close logic.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the document can be closed; otherwise, false.</returns>
        public virtual ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<bool>(true);
        }

        #endregion
    }
}
