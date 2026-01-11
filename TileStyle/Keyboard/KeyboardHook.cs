using Microsoft.Extensions.Logging;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Keyboard;

public partial class KeyboardHook : IDisposable
{
    private readonly ILogger<KeyboardHook> _logger;
    private readonly HiddenWindow _window;
    private readonly Dictionary<int, HotKey> _registeredHotKeys = new();
    private int _hookCounter;

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    public List<HotKey> RegisteredHotKeys => _registeredHotKeys
        .Select(k => k.Value)
        .Distinct()
        .ToList();


    public KeyboardHook(ILogger<KeyboardHook> logger, HiddenWindow window)
    {
        _logger = logger;
        _window = window;
        _window.HotKeyPressed += OnKeyPressed;
    }


    /// <summary>
    /// Register the hot key and return the hook id.
    /// </summary>
    /// <param name="key">The main character of the hot key.</param>
    /// <param name="modifier">The modifier key like win, alt, etc.</param>
    /// <returns>An int representing the hook id.</returns>
    public int RegisterHotKey(Keys key, ModifierKeys modifier = ModifierKeys.None)
    {
        _hookCounter++;
        RegisterHotKey(_window.Handle, _hookCounter, (uint)modifier, (uint)key);

        HotKey hotKey = new(key, modifier);
        _registeredHotKeys.Add(_hookCounter, hotKey);

        _logger.LogDebug("Registered HotKey {hotKey} with hook Id {hookId}.", hotKey, _hookCounter);

        return _hookCounter;
    }

    /// <summary>
    /// Unregister the hot key using it's hook id.
    /// </summary>
    /// <param name="hookId">The hook id corresponding to the hotkey.</param>
    public void UnregisterHotKey(int hookId)
    {
        UnregisterHotKey(_window.Handle, hookId);
        _registeredHotKeys.Remove(hookId);

        _logger.LogDebug("Unregistered HotKey with hook Id {hookId}.", hookId);
    }

    /// <summary>
    /// Unregister all events using the event hookId's.
    /// </summary>
    public void UnregisterAllHotKeys()
    {
        List<int> keys = _registeredHotKeys.Keys.ToList();
        foreach (int key in keys)
        {
            UnregisterHotKey(key);
        }
    }

    protected virtual void OnKeyPressed(object? sender, HotKeyEventArgs e)
    {
        if (!_registeredHotKeys.TryGetValue(e.HotKeyHookId, out var hotKey))
        {
            _logger.LogError("Event triggered for HotKey with hook Id {Id}. But no binding found...", e.HotKeyHookId);
            return;
        }

        KeyPressedEventArgs eventArgs = new(hotKey.ModifierKeys, hotKey.MainKey);
        KeyPressed?.Invoke(sender, eventArgs);
    }

    public void Dispose()
    {
        UnregisterHotKey(IntPtr.Zero, _hookCounter);
    }
}