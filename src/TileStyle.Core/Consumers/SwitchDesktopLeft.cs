using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SwitchDesktopLeft : IKeyConsumer
{
    private readonly VirtualDesktopHelper _helper;
    private readonly WindowManager _windowManager;

    public SwitchDesktopLeft(VirtualDesktopHelper helper, WindowManager windowManager)
    {
        _helper = helper;
        _windowManager = windowManager;
    }

    public HotKey HotKey => new(
        Keys.Left,
        ModifierKeys.Win
    );

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        _helper.SwitchToDesktop(MoveDirection.Left);
        await _windowManager.UpdateWindows();
    }
}