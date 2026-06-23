using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SwitchDesktopRight : IKeyConsumer
{
    private readonly VirtualDesktopHelper _virtualDesktopHelper;

    public SwitchDesktopRight(VirtualDesktopHelper virtualDesktopHelper)
    {
        _virtualDesktopHelper = virtualDesktopHelper;
    }

    public HotKey HotKey => new(
        Keys.Right,
        ModifierKeys.Win
    );

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        await Task.Run(() =>_virtualDesktopHelper.SwitchToDesktop(MoveDirection.Right));
    }
}