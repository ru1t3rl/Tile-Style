
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TileStyle.Models;

namespace TileStyle;

public partial class VirtualDesktopHelper
{
    private readonly IVirtualDesktopManager? _manager;
    private readonly ILogger<VirtualDesktopHelper> _logger;

    public VirtualDesktopHelper(ILogger<VirtualDesktopHelper> logger)
    {
        _logger = logger;
        try
        {
            var shell = new CVirtualDesktopManagerInternal();
            _manager = (IVirtualDesktopManager)shell;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize VirtualDesktopHelper");
            _manager = null;
        }
    }

    public Guid GetCurrentDesktop()
    {
        if (_manager == null) return Guid.Empty;

        try
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd != IntPtr.Zero)
            {
                _manager.GetWindowDesktopId(hwnd, out Guid desktopId);
                return desktopId;
            }
            return Guid.Empty;
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

    public void SwitchToDesktop(MoveDirection direction)
    {
        // Simulate Win+Ctrl+Left or Win+Ctrl+Right keyboard shortcuts
        // These are the built-in Windows shortcuts for switching virtual desktops
        
        try
        {
            // Press Win
            keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
            // Press Ctrl
            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            // Press Left or Right arrow
            byte arrowKey = direction == MoveDirection.Left ? VK_LEFT : VK_RIGHT;
            keybd_event(arrowKey, 0, 0, UIntPtr.Zero);
            
            // Small delay to ensure keys are registered
            Thread.Sleep(50);
            
            // Release Left or Right arrow
            keybd_event(arrowKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Release Ctrl
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Release Win
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            
            _logger.LogDebug("Switched desktop {Direction}", direction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch desktop");
        }
    }
}