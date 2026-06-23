using System.Runtime.InteropServices;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SwitchDesktopLeft : IKeyConsumer
{
    private readonly VirtualDesktopHelper _helper;
    private readonly WindowManager _manager;

    public SwitchDesktopLeft(VirtualDesktopHelper helper, WindowManager manager)
    {
        _helper = helper;
        _manager = manager;
    }

    public HotKey HotKey => new(
        Keys.Left,
        ModifierKeys.Win
    );

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        await Task.Run(() => _helper.SwitchToDesktop(MoveDirection.Left));
    }
}