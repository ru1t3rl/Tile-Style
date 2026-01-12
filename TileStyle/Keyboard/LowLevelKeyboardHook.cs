using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Keyboard;

public partial class LowLevelKeyboardHook : IKeyboardHook
{
    private readonly ILogger<LowLevelKeyboardHook> _logger;
    private readonly HiddenWindow _window;
    private readonly Dictionary<int, HotKey> _registeredHotKeys = new();
    private int _hookCounter;
    private IntPtr _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _hookProc;

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    public List<HotKey> RegisteredHotKeys => _registeredHotKeys
        .Select(k => k.Value)
        .Distinct()
        .ToList();

    public LowLevelKeyboardHook(ILogger<LowLevelKeyboardHook> logger, HiddenWindow window)
    {
        _logger = logger;
        _window = window;
        
        // Install low-level keyboard hook
        _hookProc = HookCallback;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(null!), 0);
        
        if (_hookId == IntPtr.Zero)
        {
            _logger.LogError("Failed to install keyboard hook");
        }
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

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            Keys key = (Keys)hookStruct.vkCode;

            ModifierKeys currentModifiers = GetCurrentModifiers();

            // Check if this key combination is registered
            foreach (var registeredHotKey in _registeredHotKeys)
            {
                if (registeredHotKey.Value.MainKey == key && registeredHotKey.Value.ModifierKeys == currentModifiers)
                {
                    _logger.LogDebug("Intercepted hotkey: {key} with modifiers {modifiers}", key, currentModifiers);
                    
                    KeyPressedEventArgs eventArgs = new(currentModifiers, key);
                    KeyPressed?.Invoke(this, eventArgs);
                    
                    // Return 1 to suppress the key press
                    return (IntPtr)1;
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private ModifierKeys GetCurrentModifiers()
    {
        ModifierKeys modifiers = ModifierKeys.None;

        if ((Control.ModifierKeys & Keys.Control) != 0)
            modifiers |= ModifierKeys.Control;
        if ((Control.ModifierKeys & Keys.Alt) != 0)
            modifiers |= ModifierKeys.Alt;
        if ((Control.ModifierKeys & Keys.Shift) != 0)
            modifiers |= ModifierKeys.Shift;
        if ((GetAsyncKeyState((int)Keys.LWin) & 0x8000) != 0 || (GetAsyncKeyState((int)Keys.RWin) & 0x8000) != 0)
            modifiers |= ModifierKeys.Win;

        return modifiers;
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
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }
}