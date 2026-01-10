using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SplitHorizontal : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public KeyCombination HotKey => new(
        Keys.H,
        ModifierKeys.Win
    );

    public SplitHorizontal(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        _windowManager.SplitHorizontal();
        return Task.CompletedTask;
    }
}