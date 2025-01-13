using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace System.Windows.Controls
{
    public static class VirtualizingStackPanelHelper
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo?> s_bringIndexIntoViewMethods = new();

        static VirtualizingStackPanelHelper()
        {

        }

        /// <summary>
        /// Publically expose BringIndexIntoView.
        /// </summary>
        public static void BringIntoView(this VirtualizingStackPanel virtualizingPanel, int index)
        {
#if NET
            ArgumentNullException.ThrowIfNull(virtualizingPanel);
#else
            Throw.IfNull(virtualizingPanel);
#endif
            var mi = s_bringIndexIntoViewMethods.GetOrAdd(virtualizingPanel.GetType(), type => virtualizingPanel.GetType().GetMethod("BringIndexIntoView",
                BindingFlags.NonPublic | BindingFlags.Instance));
            Debug.Assert(mi != null);

            //virtualizingPanel.BringIndexIntoView(index);
            mi?.Invoke(virtualizingPanel, new object[] { index });//TODO to delegate
        }
    }
}
