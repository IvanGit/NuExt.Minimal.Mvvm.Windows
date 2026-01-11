using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Defines the contract for a window ViewModel.
    /// </summary>
    public interface IWindowViewModel : IControlViewModel
    {
        /// <summary>
        /// Gets the cancellation token for window lifecycle.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets or sets the window title.
        /// </summary>
        string? Title { get; set; }

        /// <summary>
        /// Gets the command to close the window.
        /// </summary>
        ICommand? CloseCommand { get; }

        /// <summary>
        /// Closes the window asynchronously and disposes this ViewModel.
        /// </summary>
        /// <param name="force">If true, forces the window to close.</param>
        ValueTask CloseAsync(bool force = false);

    }
}
