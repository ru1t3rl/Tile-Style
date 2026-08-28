using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveToDesktopRight : IKeyConsumer
{
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.Right,
        ModifierKeys.Win | ModifierKeys.Alt | ModifierKeys.Control
    );

    public MoveToDesktopRight(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        await Task.Run(() => _windowManager.MoveFocusedWindowToDesktop(MoveDirection.Right));
    }
}