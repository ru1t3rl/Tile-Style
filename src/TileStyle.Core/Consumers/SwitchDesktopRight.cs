using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers;

public class SwitchDesktopRight : IKeyConsumer
{
    private readonly VirtualDesktopHelper _virtualDesktopHelper;
    private readonly WindowManager _windowManager;

    public SwitchDesktopRight(VirtualDesktopHelper virtualDesktopHelper, WindowManager windowManager)
    {
        _virtualDesktopHelper = virtualDesktopHelper;
        _windowManager = windowManager;
    }

    public HotKey HotKey => new(
        Keys.Right,
        ModifierKeys.Win
    );

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        _virtualDesktopHelper.SwitchToDesktop(MoveDirection.Right);
        await _windowManager.UpdateWindows();
    }
}