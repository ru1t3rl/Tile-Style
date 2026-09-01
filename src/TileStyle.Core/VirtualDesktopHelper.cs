using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TileStyle.Models;
using TileStyle.Windows;
using WindowsDesktop;

namespace TileStyle;

public partial class VirtualDesktopHelper
{
    private readonly ILogger<VirtualDesktopHelper> _logger;
    private readonly IServiceProvider _serviceProvider;

    public VirtualDesktopHelper(ILogger<VirtualDesktopHelper> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Guid GetCurrentDesktop()
    {
        try
        {
            return VirtualDesktop.Current.Id;
        }
        catch
        {
            return Guid.Empty;
        }
    }

    public Guid GetWindowDesktop(IntPtr hwnd)
    {
        try
        {
            var desktop = VirtualDesktop.FromHwnd(hwnd);
            return desktop?.Id ?? Guid.Empty;
        }
        catch
        {
            _logger.LogError("Failed to get window desktop {hwnd}.", hwnd);
            return Guid.Empty;
        }
    }

    public void RemoveDesktopIfEmpty(Guid desktopId)
    {
        VirtualDesktop? desktop = VirtualDesktop.FromId(desktopId);

        WindowStore store = _serviceProvider.GetRequiredService<WindowStore>();
        store.DesktopGroupedWindows.TryGetValue(desktopId, out List<Window>? windows);

        if (desktop is not null &&
            (windows is null ||
             windows?.Count <= 0)
           )
        {
            desktop?.Remove();
        }
    }

    public void SwitchToDesktop(MoveDirection direction, bool removeOldIfEmpty = true)
    {
        var current = VirtualDesktop.Current;
        var target = direction switch
        {
            MoveDirection.Left => current.GetLeft(),
            MoveDirection.Right => current.GetRight(),
            _ => null
        };

        Guid currentDesktopId = current?.Id ?? Guid.Empty;

        if (direction is MoveDirection.Left && target is null)
        {
            _logger.LogError("Failed to move to desktop on the left.");
            return;
        }

        if (direction is MoveDirection.Right && target is null)
        {
            target = VirtualDesktop.Create();
        }

        target?.Switch();
        RemoveDesktopIfEmpty(currentDesktopId);
    }

    public void MoveWindowToDesktop(IntPtr hwnd, Guid desktopId)
    {
        try
        {
            VirtualDesktop? desktop = VirtualDesktop.GetDesktops().FirstOrDefault(d => d.Id == desktopId);

            if (desktop is null)
            {
                _logger.LogError("Failed to move window {hwnd} to desktop {DesktopId} it couldn't be found.", hwnd,
                    desktopId);
                return;
            }

            VirtualDesktop.MoveToDesktop(hwnd, desktop);
        }
        catch
        {
            // Failed to move window
        }
    }

    public void MoveWindowToNextDesktop(nint hwnd, MoveDirection direction)
    {
        var current = VirtualDesktop.FromHwnd(hwnd);
        if (current is null)
        {
            _logger.LogError("Failed to move window {hwnd} to current desktop couldn't be found.", hwnd);
            return;
        }

        var target = direction switch
        {
            MoveDirection.Left => current.GetLeft(),
            MoveDirection.Right => current.GetRight(),
            _ => null
        };

        if (direction is MoveDirection.Left && target is null)
        {
            _logger.LogError("Failed to move window {hwnd} to left desktop couldn't be found.", hwnd);
            return;
        }

        if (direction is MoveDirection.Right && target is null)
        {
            target = VirtualDesktop.Create();
        }

        if (target is null)
        {
            _logger.LogError("Unsupported window move direction {direction}.", direction);
            return;
        }

        VirtualDesktop.MoveToDesktop(hwnd, target);
        target.Switch();
    }
}