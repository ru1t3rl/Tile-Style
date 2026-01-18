namespace TileStyle.Windows;

public partial class WindowStyleChanger
{
    internal void SetWindowStyle(IntPtr windowHandle, DwmCornerPreference style)
    {
        DwmSetWindowAttribute(
            windowHandle,
            DwmWindowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE,
            ref style,
            sizeof(uint));
    }

    public void SwitchToFloatingStyle(IntPtr windowHandle)
    {
        SetWindowStyle(windowHandle, DwmCornerPreference.DWMWCP_ROUND);
    }

    public void SwitchToFullHeightStyle(IntPtr windowHandle)
    {
        SetWindowStyle(windowHandle, DwmCornerPreference.DWMWCP_DONOTROUND);
    }
}