using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveToDesktopLeft : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public KeyCombination HotKey => new(
        Keys.Left,
        ModifierKeys.Win | ModifierKeys.Control
    );

    public MoveToDesktopLeft(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        _windowManager.MoveWindowToDesktop((int)MoveDirection.Left);
        return Task.CompletedTask;
    }
}