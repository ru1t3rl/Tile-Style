using System.Runtime.InteropServices;

namespace TileStyle.Windows;

public partial class WindowStyleChanger
{
    internal  enum DWMWINDOWATTRIBUTE : uint
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    internal  enum DWM_WINDOW_CORNER_PREFERENCE : uint
    {
        DWMWCP_DEFAULT    = 0,  // System default
        DWMWCP_DONOTROUND = 1,  // Never round
        DWMWCP_ROUND      = 2,  // Round if appropriate
        DWMWCP_ROUNDSMALL = 3   // Small rounding
    }

    
    [DllImport("dwmapi.dll")]
    private extern static int DwmSetWindowAttribute(
        IntPtr hwnd,
        DWMWINDOWATTRIBUTE attribute,
        ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
        uint cbAttribute);
}