using System.Runtime.InteropServices;
using System.Text;

namespace TileStyle;

public partial class WindowManager
{
    private const int WM_CLOSE = 0x0010;
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll")]
    private extern static bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);


    [DllImport("user32.dll")]
    private extern static IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private extern static int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private extern static bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private extern static IntPtr GetForegroundWindow();

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal struct InternalRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}