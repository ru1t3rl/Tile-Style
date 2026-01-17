using TileStyle.Consumers;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Actions;

public class FocusNext : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.J,
        ModifierKeys.Win
    );

    public FocusNext(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        // _windowManager.FocusNext();
        return Task.CompletedTask;
    }
}