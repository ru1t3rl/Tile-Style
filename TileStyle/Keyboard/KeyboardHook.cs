using TileStyle.Window;

namespace TileStyle.Keyboard;

public partial class KeyboardHook : IDisposable
{
    private readonly HiddenWindow _window;
    private int _hookId;

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    public KeyboardHook(HiddenWindow window)
    {
        _window = window;
        _window.HotKeyPressed += OnKeyPressed()
    }


    /// <summary>
    /// Register the hot key and return the hook id.
    /// </summary>
    /// <param name="key">The main character of the hot key.</param>
    /// <param name="modifier">The modifier key like win, alt, etc.</param>
    /// <returns>An int representing the hook id.</returns>
    public int RegisterHotKey(Keys key, ModifierKeys modifier = ModifierKeys.None)
    {
        _hookId++;
        RegisterHotKey(_window.Handle, _hookId, (uint)modifier, (uint)key);
        return _hookId;
    }

    /// <summary>
    /// Unregister the hot key using it's hook id.
    /// </summary>
    /// <param name="hookId">The hook id corresponding to the hotkey.</param>
    public void UnregisterHotKey(int hookId)
    {
        UnregisterHotKey(_window.Handle, hookId);
    }

    protected virtual void OnKeyPressed(HotKeyEventArgs e)
    {
        KeyPressed?.Invoke(this, e);
    }

    public void Dispose()
    {
        UnregisterHotKey(IntPtr.Zero, _hookId);
    }
}