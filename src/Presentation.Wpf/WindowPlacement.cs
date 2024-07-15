using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

using static Windows.Win32.PInvoke;

namespace System.Windows
{
    /// <summary>
    /// The `WindowPlacement` class is used to manage the position and state of windows in Windows applications.
    /// It allows obtaining the current position, size, and state of a window, as well as modifying them.
    /// </summary>
    public class WindowPlacement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the flags that specify the window's state.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the minimum position point of the window when minimized.
        /// </summary>
        public System.Drawing.Point MinPosition { get; set; }

        /// <summary>
        /// Gets or sets the maximum position point of the window when maximized.
        /// </summary>
        public System.Drawing.Point MaxPosition { get; set; }

        /// <summary>
        /// Gets or sets the normal (restored) position and size of the window.
        /// </summary>
        public Rectangle NormalPosition { get; set; }

        /// <summary>
        /// Gets or sets the show command which determines how the window is to be shown.
        /// The default value is SW_SHOWNORMAL.
        /// </summary>
        public int ShowCmd { get; set; } = (int)SHOW_WINDOW_CMD.SW_SHOWNORMAL;

        /// <summary>
        /// Gets or sets the resize mode for the window. The default is CanResize.
        /// </summary>
        public ResizeMode ResizeMode { get; set; } = ResizeMode.CanResize;

        /// <summary>
        /// Gets or sets the window style. The default is SingleBorderWindow.
        /// </summary>
        public WindowStyle WindowStyle { get; set; } = WindowStyle.SingleBorderWindow;

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the current placement (position and state) of the specified window.
        /// </summary>
        /// <param name="windowHandle">The handle of the window.</param>
        /// <returns>An instance of <see cref="WindowPlacement"/> with the current values of the window's placement,
        /// or <c>null</c> if the operation fails.</returns>
        public static WindowPlacement? GetPlacement(nint windowHandle)
        {
            var placement = new WINDOWPLACEMENT()
            {
                length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>()
            };
            if (!GetWindowPlacement((HWND)windowHandle, ref placement))
            {
                return null;
            }
            return new WindowPlacement()
            {
                Flags = (uint)placement.flags,
                MinPosition = placement.ptMinPosition,
                MaxPosition = placement.ptMaxPosition,
                NormalPosition = placement.rcNormalPosition,
                ShowCmd = (int)placement.showCmd,
            };
        }

        /// <summary>
        /// Sets the placement (position and state) of the specified window according to the provided <see cref="WindowPlacement"/> instance.
        /// </summary>
        /// <param name="windowHandle">The handle of the window.</param>
        /// <param name="windowPlacement">An instance of <see cref="WindowPlacement"/> specifying the new values for the window's placement.</param>
        /// <returns><c>true</c> if the operation is successful, otherwise <c>false</c>.</returns>
        public static bool SetPlacement(nint windowHandle, WindowPlacement? windowPlacement)
        {
            if (windowPlacement is null)
            {
                return false;
            }
            try
            {
                var placement = new WINDOWPLACEMENT()
                {
                    length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
                    flags = (WINDOWPLACEMENT_FLAGS)windowPlacement.Flags,
                    ptMinPosition = windowPlacement.MinPosition,
                    ptMaxPosition = windowPlacement.MaxPosition,
                    rcNormalPosition = windowPlacement.NormalPosition,
                    showCmd = (SHOW_WINDOW_CMD)windowPlacement.ShowCmd
                };

                placement.showCmd = (placement.showCmd == SHOW_WINDOW_CMD.SW_SHOWMINIMIZED ?
                    (placement.flags == WINDOWPLACEMENT_FLAGS.WPF_RESTORETOMAXIMIZED ? SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED : SHOW_WINDOW_CMD.SW_SHOWNORMAL) :
                    placement.showCmd);
                return SetWindowPlacement((HWND)windowHandle, in placement);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
            return false;
        }

        #endregion
    }
}
