namespace System.Windows.Controls
{
    public static class ControlHelper
    {
        public static void ClearStyle(this Control? control)
        {
            if (control is null)
            {
                return;
            }

            control.Template = null;
            control.Style = null;
        }
    }
}
