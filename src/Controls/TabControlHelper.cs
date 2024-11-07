using System.Windows.Controls;

namespace Minimal.Mvvm.Windows.Controls
{
    public static class TabControlHelper
    {
        public static void ClearStyle(this TabItem? tabItem)
        {
            if (tabItem is null)
            {
                return;
            }

            tabItem.Template = null;
            tabItem.Style = null;
        }
    }
}
