using System.Collections.Frozen;
using TileStyle.Consumers;
using TileStyle.Models;
using TileStyle.Common;

namespace TileStyle.Keyboard;

public class KeyboardEventService : BackgroundService
{
    private readonly FrozenDictionary<HotKey, IKeyConsumer[]> _keyConsumers;
    private readonly KeyboardHook _keyboardHook;
    private readonly List<int> _keyboardHookIds = new();

    private Thread? _messageLoopThread;

    public KeyboardEventService(IEnumerable<IKeyConsumer> keyConsumers, KeyboardHook keyboardHook)
    {
        _keyboardHook = keyboardHook;
        _keyConsumers = keyConsumers
            .GroupBy(k => k.HotKey)
            .ToFrozenDictionary(
                k => k.Key,
                k => k.Select(kc => kc).ToArray()
            );
    }

    private void RegisterKeyCombinations()
    {
        foreach (HotKey keyBinding in _keyConsumers.Keys)
        {
            int hookId = _keyboardHook.RegisterHotKey(keyBinding.MainKey, keyBinding.ModifierKeys);
            _keyboardHookIds.Add(hookId);
        }
    }

    private void UnRegisterKeyCombinations()
    {
        foreach (int hookId in _keyboardHookIds)
        {
            _keyboardHook.UnregisterHotKey(hookId);
        }

        _keyboardHookIds.Clear();
    }

    protected override Task Initialize(CancellationToken cancellationToken)
    {
        RegisterKeyCombinations();
        _keyboardHook.KeyPressed += OnKeyPressed;
        return Task.CompletedTask;
    }

    protected override Task Dispose(CancellationToken cancellationToken)
    {
        UnRegisterKeyCombinations();
        _keyboardHook.KeyPressed -= OnKeyPressed;
        return Task.CompletedTask;
    }

    private async void OnKeyPressed(object? sender, KeyPressedEventArgs e)
    {
        HotKey hotKey = new(e.Key, e.Modifier);
        if (!_keyConsumers.TryGetValue(hotKey, out IKeyConsumer[]? consumers))
        {
            return;
        }

        foreach (IKeyConsumer keyConsumer in consumers)
        {
            await keyConsumer.ExecuteAsync(sender, e);
        }
    }
}