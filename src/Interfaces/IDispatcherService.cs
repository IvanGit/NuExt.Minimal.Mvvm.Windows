using System.Threading;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Service interface for thread synchronization and dispatcher operations.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="ISynchronizeInvoker"/> to provide standardized dispatcher access
    /// for cross-thread UI operations in WPF applications.
    /// </remarks>
    public interface IDispatcherService : ISynchronizeInvoker
    {
    }
}
