using System.Runtime.InteropServices;

namespace TileStyle.Windows;

internal enum DwmWindowAttribute : uint
{
    DWMWA_WINDOW_CORNER_PREFERENCE = 33
}

internal enum DwmCornerPreference : uint
{
    DWMWCP_DEFAULT    = 0,  // System default
    DWMWCP_DONOTROUND = 1,  // Never round
    DWMWCP_ROUND      = 2,  // Round if appropriate
    DWMWCP_ROUNDSMALL = 3   // Small rounding
}

public partial class WindowStyleChanger
{
    [DllImport("dwmapi.dll")]
    private extern static int DwmSetWindowAttribute(
        IntPtr hwnd,
        DwmWindowAttribute attribute,
        ref DwmCornerPreference pvAttribute,
        uint cbAttribute);
}