using System.Runtime.InteropServices;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SwitchDesktopLeft : IKeyConsumer
{
    private readonly VirtualDesktopHelper _helper;

    public SwitchDesktopLeft(VirtualDesktopHelper helper)
    {
        _helper = helper;
    }

    public HotKey HotKey => new(
        Keys.Left,
        ModifierKeys.Win
    );

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        _helper.SwitchToDesktop(MoveDirection.Left);
        return Task.CompletedTask;
    }
}