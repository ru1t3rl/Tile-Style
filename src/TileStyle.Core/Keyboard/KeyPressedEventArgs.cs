namespace TileStyle.Keyboard;

public class KeyPressedEventArgs : EventArgs
{
    public ModifierKeys Modifier { get; }
    public Keys Key { get; }

    public KeyPressedEventArgs(ModifierKeys modifier, Keys key)
    {
        Modifier = modifier;
        Key = key;
    }
}