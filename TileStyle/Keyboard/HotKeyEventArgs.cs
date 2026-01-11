namespace TileStyle.Keyboard;

public class HotKeyEventArgs : EventArgs
{
    public int HotKeyHookId { get; init; }

    public HotKeyEventArgs(int hotKeyHookId)
    {
        HotKeyHookId = hotKeyHookId;
    }
}