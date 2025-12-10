using System.Diagnostics;
using System.Linq;

namespace Minimal.Mvvm.Windows
{
    partial class ControlViewModel
    {
        [Conditional("DEBUG")]
        private void ValidateDisposingState()
        {
            var typeName = GetType().FullName;
            var displayName = DisplayName ?? "Unnamed";
            var hashCode = GetHashCode();

            Debug.Assert(CheckAccess());

            var asyncCommands = GetAllAsyncCommands();
            Debug.Assert(asyncCommands.Count == 0 ||
                         asyncCommands.All(pair => pair.Command.IsExecuting == false || pair.Command.IsCancellationRequested),
                $"{typeName} ({displayName}) ({hashCode}) has unexpected state of async commands.");
        }

        [Conditional("DEBUG")]
        private void ValidateFinalState()
        {
            var typeName = GetType().FullName;
            var displayName = DisplayName ?? "Unnamed";
            var hashCode = GetHashCode();

            var commands = this.GetAllCommands();
            Debug.Assert(commands.All(c => c.Command is null), $"{typeName} ({displayName}) ({hashCode}) has not nullified commands.");
        }
    }
}
