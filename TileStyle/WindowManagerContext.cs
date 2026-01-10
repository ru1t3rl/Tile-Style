using TileStyle.Keyboard;

namespace TileStyle;

public class WindowManagerContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly WindowManager _windowManager;

    public WindowManagerContext(WindowManager windowManager)
    {
        _windowManager = windowManager;
        
        _trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true,
            Text = "Tiling Window Manager"
        };

        _trayIcon.ContextMenuStrip.Items.Add("Toggle Tiling", null, (_, _) => _windowManager.ToggleTiling());
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => Exit());
    }

    private void Exit()
    {
        _trayIcon.Visible = false;
        Application.Exit();
    }
}