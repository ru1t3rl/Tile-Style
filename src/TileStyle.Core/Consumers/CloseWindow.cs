using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers;

public class CloseWindow : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.Q,
        ModifierKeys.Win
    );

    public CloseWindow(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        WindowEventArgs eventArgs = new(_windowManager.ActiveWindowHandle);
        _windowManager.CloseWindow(sender, eventArgs);
        return Task.CompletedTask;
    }
}