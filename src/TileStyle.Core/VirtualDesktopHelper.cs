using Microsoft.Extensions.Logging;
using TileStyle.Models;
using WindowsDesktop;

namespace TileStyle;

public partial class VirtualDesktopHelper
{
    private readonly ILogger<VirtualDesktopHelper> _logger;

    public VirtualDesktopHelper(ILogger<VirtualDesktopHelper> logger)
    {
        _logger = logger;
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
            return Guid.Empty;
        }
    }

    public void SwitchToDesktop(MoveDirection direction)
    {
        var current = VirtualDesktop.Current;
        var target = direction switch
        {
            MoveDirection.Left => current.GetLeft(),
            MoveDirection.Right => current.GetRight(),
            _ => null
        };

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
    }

    public void MoveWindowToDesktop(IntPtr hwnd, Guid desktopId)
    {
        try
        {
            VirtualDesktop? desktop = VirtualDesktop.GetDesktops().FirstOrDefault(d => d.Id == desktopId);

            if (desktop is null)
            {
                _logger.LogError("Failed to move window {hwnd} to desktop {DesktopId} it couldn't be found.", hwnd, desktopId);
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