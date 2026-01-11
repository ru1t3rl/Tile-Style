using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SplitVertical : IKeyConsumer
{
    private readonly WindowManager _windowManager;
    public HotKey HotKey => new(
        Keys.V,
        ModifierKeys.Win
    );

    public SplitVertical(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        // _windowManager.SplitVertical();
        return Task.CompletedTask;
    }
}