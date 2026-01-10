namespace TileStyle.Keyboard;

public class HotKeyEventArgs : EventArgs
{
    public int HotKeyId { get; init; }

    public HotKeyEventArgs(int hotKeyId)
    {
        HotKeyId = hotKeyId;
    }
}