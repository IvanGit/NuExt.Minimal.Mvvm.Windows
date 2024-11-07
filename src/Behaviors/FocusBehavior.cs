using System.Diagnostics;
using System.Windows.Controls;

namespace Minimal.Mvvm.Windows
{
    /// <summary>
    /// Represents a behavior that sets focus on a control when a specified event is triggered.
    /// </summary>
    public class FocusBehavior : EventTriggerBase<Control>
    {
        protected override void OnEvent(object? sender, object? eventArgs)
        {
            Debug.Assert(sender is Control);
            if (sender is not Control element)
            {
                return;
            }
            if (element is { Focusable: true, IsTabStop: true })
            {
                element.Focus();
            }
        }
    }
}
