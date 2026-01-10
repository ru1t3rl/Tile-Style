using System.Runtime.InteropServices;

namespace TileStyle;

public partial class VirtualDesktopHelper
{
    [DllImport("user32.dll")]
    private extern static IntPtr GetForegroundWindow();

    [ComImport]
    [Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
    private class VirtualDesktopManagerInternal
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    private interface IVirtualDesktopManager
    {
        bool IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);
        Guid GetWindowDesktopId(IntPtr topLevelWindow, out Guid desktopId);
        void MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
    }
}