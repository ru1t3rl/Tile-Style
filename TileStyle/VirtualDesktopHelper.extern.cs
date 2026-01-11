using System.Runtime.InteropServices;

namespace TileStyle;

public partial class VirtualDesktopHelper
{
    [DllImport("user32.dll")]
    private extern static IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    // Virtual key codes
    private const byte VK_LWIN = 0x5B;
    private const byte VK_CONTROL = 0x11;
    private const byte VK_LEFT = 0x25;
    private const byte VK_RIGHT = 0x27;
    
    // Key event flags
    private const uint KEYEVENTF_KEYUP = 0x0002;

    [ComImport]
    [Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IVirtualDesktopManager
    {
        bool IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);
        Guid GetWindowDesktopId(IntPtr topLevelWindow, out Guid desktopId);
        void MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
    }

    [ComImport]
    [Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A")]
    private class CVirtualDesktopManagerInternal { }
}