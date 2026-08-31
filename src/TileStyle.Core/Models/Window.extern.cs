using System.Runtime.InteropServices;
using System.Text;

namespace TileStyle.Models;

public partial class Window
{
    private const uint SWP_ASYNCWINDOWPOS = 0x4000;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    
    [DllImport("user32.dll")]
    private extern static bool GetWindowRect(IntPtr hWnd, out WindowManager.InternalRect lpRect);
    
    [DllImport("user32.dll")]
    private extern static bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private extern static int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private extern static int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private extern static bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private extern static bool IsIconic(IntPtr hWnd);
}