using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class CloseWindow : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public KeyCombination HotKey => new(
        Keys.Q,
        ModifierKeys.Win
    );

    public CloseWindow(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        _windowManager.CloseActiveWindow();
        return Task.CompletedTask;
    }
}