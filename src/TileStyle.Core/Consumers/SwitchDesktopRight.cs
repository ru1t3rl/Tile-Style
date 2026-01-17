using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SwitchDesktopRight : IKeyConsumer
{
    private readonly VirtualDesktopHelper _helper;

    public SwitchDesktopRight(VirtualDesktopHelper helper)
    {
        _helper = helper;
    }

    public HotKey HotKey => new(
        Keys.Right,
        ModifierKeys.Win
    );

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        _helper.SwitchToDesktop(MoveDirection.Right);
        return Task.CompletedTask;
    }
}