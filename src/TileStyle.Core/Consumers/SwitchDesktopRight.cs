using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers;

public class SwitchDesktopRight : IKeyConsumer
{
    private readonly VirtualDesktopHelper _virtualDesktopHelper;
    private readonly WindowStore _windowStore;

    public SwitchDesktopRight(VirtualDesktopHelper virtualDesktopHelper, WindowStore windowStore)
    {
        _virtualDesktopHelper = virtualDesktopHelper;
        _windowStore = windowStore;
    }

    public HotKey HotKey => new(
        Keys.Right,
        ModifierKeys.Win
    );

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        _virtualDesktopHelper.SwitchToDesktop(MoveDirection.Right);
    }
}