using System.Diagnostics;

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
            Debug.Assert(_asyncCommands.IsEmpty ||
                         _asyncCommands.All(pair => pair.Value.IsExecuting == false || pair.Value.IsCancellationRequested),
                $"{typeName} ({displayName}) ({hashCode}) has unexpected state of async commands.");
        }

        [Conditional("DEBUG")]
        private void ValidateFinalState()
        {
            var typeName = GetType().FullName;
            var displayName = DisplayName ?? "Unnamed";
            var hashCode = GetHashCode();

            Debug.Assert(_asyncCommands.IsEmpty, $"{typeName} ({displayName}) ({hashCode}) has {_asyncCommands.Count} registered commands.");

            var commands = GetAllCommands();
            Debug.Assert(commands.All(c => c.Value is null));

            Debug.Assert(CheckAccess());
        }
    }
}
