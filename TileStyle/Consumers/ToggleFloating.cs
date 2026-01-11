using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class ToggleFloating : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.F,
        ModifierKeys.Win
    );

    public ToggleFloating(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        // _windowManager.ToggleFloating();
        return Task.CompletedTask;
    }
}