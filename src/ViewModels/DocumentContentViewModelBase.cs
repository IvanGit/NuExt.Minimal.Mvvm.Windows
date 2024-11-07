namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Provides a base class for view models that represent document content. 
    /// This class extends <see cref="ControlViewModel"/>.
    /// </summary>
    public abstract class DocumentContentViewModelBase : ControlViewModel
    {
        #region Properties

        private string? _title;
        /// <summary>
        /// Gets or sets the title of the document.
        /// </summary>
        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value, t =>
            {
                var newTitle = Title;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the document can be closed. Override this method to provide custom close logic.
        /// </summary>
        /// <returns>True if the document can be closed; otherwise, false.</returns>
        public virtual bool CanClose()
        {
            return true;
        }

        #endregion
    }
}
