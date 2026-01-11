using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class FocusPrevious : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.K,
        ModifierKeys.Win
    );

    public FocusPrevious(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        // _windowManager.FocusPrevious();
        return Task.CompletedTask;
    }
}