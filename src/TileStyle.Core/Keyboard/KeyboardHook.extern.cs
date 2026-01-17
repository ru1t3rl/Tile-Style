using System.Runtime.InteropServices;

namespace TileStyle.Keyboard;

public partial class KeyboardHook
{
    [DllImport("user32.dll")]
    private extern static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private extern static bool UnregisterHotKey(IntPtr hWnd, int id);
}