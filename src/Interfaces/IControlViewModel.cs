using System.Threading;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Base ViewModel interface for UI controls with thread synchronization support.
    /// </summary>
    public interface IControlViewModel : ISynchronizeInvoker
    {
    }
}
