namespace TileStyle.Window;

public class WindowEventArgs : EventArgs
{
    public IntPtr WindowHandle { get; }

    public WindowEventArgs(IntPtr hwnd)
    {
        WindowHandle = hwnd;
    }
}