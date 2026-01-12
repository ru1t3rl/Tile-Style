using TileStyle.Models;

namespace TileStyle.Keyboard;

public interface IKeyboardHook : IDisposable
{
    event EventHandler<KeyPressedEventArgs>? KeyPressed;

    List<HotKey> RegisteredHotKeys { get; }

    int RegisterHotKey(Keys key, ModifierKeys modifier = ModifierKeys.None);

    void UnregisterHotKey(int hookId);

    void UnregisterAllHotKeys();
}