namespace TileStyle.Windows;

public partial class WindowStyleChanger
{
    internal void SetWindowStyle(IntPtr windowHandle, DWM_WINDOW_CORNER_PREFERENCE style)
    {
        DwmSetWindowAttribute(
            windowHandle,
            DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
            ref style,
            sizeof(uint));
    }

    public void SwitchToFloatingStyle(IntPtr windowHandle)
    {
        SetWindowStyle(windowHandle, DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND);
    }

    public void SwitchToFullHeightStyle(IntPtr windowHandle)
    {
        SetWindowStyle(windowHandle, DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND);
    }
}