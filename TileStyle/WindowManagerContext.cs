using TileStyle.Common;
using TileStyle.Keyboard;

namespace TileStyle;

public class WindowManagerContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly WindowManager _windowManager;
    private readonly ServiceInstaller _serviceInstaller;

    private const string APP_NAME = "TileStyle";
    private string AppName => Application.ProductName ?? APP_NAME;
    
    public WindowManagerContext(WindowManager windowManager, ServiceInstaller serviceInstaller)
    {
        _serviceInstaller = serviceInstaller;
        _windowManager = windowManager;

        _trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip(),
            Visible = true,
            Text = "Tiling Window Manager"
        };
        
        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        if (_serviceInstaller.IsInstalled(AppName))
        {
            _trayIcon.ContextMenuStrip.Items.Add("Remove from startup").Click += (_, __) => UninstallFromStartup();
        }
        else
        {
            _trayIcon.ContextMenuStrip.Items.Add("Add to startup").Click += (_, __) => InstallAppInStartup();
        }

        _trayIcon.ContextMenuStrip.Items.Add("Toggle Tiling", null, (_, _) => _windowManager.ToggleTiling());
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => Exit());
    }
    
    private void InstallAppInStartup()
    {
        _serviceInstaller.Install(
            AppName,
            Application.ExecutablePath
        );
        
        _trayIcon.ContextMenuStrip.Items.Clear();
        BuildContextMenu();
    }

    private void UninstallFromStartup()
    {
        _serviceInstaller.Uninstall(AppName);
        
        _trayIcon.ContextMenuStrip.Items.Clear();
        BuildContextMenu();
    }

    private void Exit()
    {
        _trayIcon.Visible = false;
        Application.Exit();
    }
}