using System.Runtime.InteropServices;

namespace TileStyle;

public partial class VirtualDesktopHelper
{
    private readonly IVirtualDesktopManager? _manager;

    public VirtualDesktopHelper()
    {
        try
        {
            VirtualDesktopManagerInternal shell = new();
            _manager = (IVirtualDesktopManager)shell;
        }
        catch
        {
            _manager = null;
        }
    }

    public Guid GetCurrentDesktop()
    {
        if (_manager == null) return Guid.Empty;

        try
        {
            IntPtr hwnd = GetForegroundWindow();
            _manager.GetWindowDesktopId(hwnd, out Guid desktopId);
            return desktopId;
        }
        catch
        {
            return Guid.Empty;
        }
    }

    public Guid GetWindowDesktop(IntPtr hwnd)
    {
        if (_manager == null) return Guid.Empty;

        try
        {
            _manager.GetWindowDesktopId(hwnd, out Guid desktopId);
            return desktopId;
        }
        catch
        {
            return Guid.Empty;
        }
    }
    
    public void MoveWindowToDesktop(IntPtr hwnd, Guid desktopId)
    {
        if (_manager == null) return;
            
        try
        {
            _manager.MoveWindowToDesktop(hwnd, ref desktopId);
        }
        catch
        {
            // Failed to move window
        }
    }
}