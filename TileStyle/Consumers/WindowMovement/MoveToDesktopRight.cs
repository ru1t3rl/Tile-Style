using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveToDesktopRight : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public KeyCombination HotKey => new(
        Keys.Right,
        ModifierKeys.Win | ModifierKeys.Control
    );

    public MoveToDesktopRight(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        _windowManager.MoveWindowToDesktop((int)MoveDirection.Right);
        return Task.CompletedTask;
    }
}