using TileStyle.Keyboard;

namespace TileStyle.Windows;

public class HiddenWindow : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    
    public event EventHandler<HotKeyEventArgs>? HotKeyPressed;

    public HiddenWindow()
    {
        base.CreateHandle(new CreateParams());
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            int id = m.WParam.ToInt32();
            HotKeyPressed?.Invoke(this, new HotKeyEventArgs(id));
        }

        base.WndProc(ref m);
    }

    public void Dispose()
    {
        DestroyHandle();
    }
}