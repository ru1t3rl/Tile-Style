using Microsoft.Extensions.Logging;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle;

public partial class WindowManager : IDisposable
{
    private readonly ILogger<WindowManager> _logger;
    private readonly WindowEventHook _windowEventHook;
    private SynchronizationContext? _synchronizationContext;
    private readonly VirtualDesktopHelper _virtualDesktop;

    private readonly List<Window> _windows = new();
    private readonly List<Zone> _zones = new();

    private bool _tilingEnabled = true;
    private IntPtr _activeWindowHandle = IntPtr.Zero;

    public WindowManager(WindowEventHook windowEventHook, VirtualDesktopHelper virtualDesktop, ILogger<WindowManager> logger)
    {
        _windowEventHook = windowEventHook;
        _virtualDesktop = virtualDesktop;
        _logger = logger;

        _windowEventHook.WindowCreated += AddNewWindow;
        _windowEventHook.WindowDestroyed += CloseWindow;
        _windowEventHook.WindowShown += AddNewWindow;
        _windowEventHook.WindowMinimized += CloseWindow;
        // _windowEventHook.WindowRestored += OnWindowEvent;
    }

    public void InitializeContext()
    {
        _synchronizationContext = SynchronizationContext.Current;
        UpdateWindows();
    }

    public void ToggleTiling()
    {
        _tilingEnabled = !_tilingEnabled;
        if (!_tilingEnabled)
        {
            UpdateWindows();
        }
    }

    // private void OnWindowEvent(object? sender, WindowEventArgs e)
    // {
    //     var window = new Window(e.WindowHandle);
    //     _logger.LogDebug("Update triggered by {title}", window.Title);
    //
    //     // Delay slightly to let window state stabilize
    //     Task
    //         .Delay(50)
    //         .ContinueWith(_ => { _synchronizationContext?.Post(_ => UpdateWindows(), null); });
    // }

    private void AddNewWindow(object? sender, WindowEventArgs e)
    {
        Window window = new Window(e.WindowHandle);
        Guid currentDesktopId = _virtualDesktop.GetCurrentDesktop();

        if (
            !window.Visible ||
            window.Floating ||
            window.Minimized ||
            !ShouldManageWindow(window, currentDesktopId) ||
            IsManagedWindow(window.Handle)
        )
        {
            return;
        }
        
        // Find best fitting zone
        // Check if new zone is required
        // Add window to zone
    }

    private void CloseWindow(object? sender, WindowEventArgs e)
    {
    }

    /// <summary>
    /// The update windows function performs a full refresh of the entire manager environment.
    /// This will clear/reset the <see cref="_zones"/> and <see cref="_windows"/> lists.
    /// </summary>
    private void UpdateWindows()
    {
        if (!_tilingEnabled)
        {
            return;
        }

        List<Window> windows = GetWindows().OrderBy(w => w.Position.X).ToList();
        _windows.Clear();
        _windows.AddRange(windows);

        int newZoneCount = (int)Math.Max(1, Math.Floor(_windows.Count / 2f));
        int windowsPerZone = _windows.Count / newZoneCount;

        Guid desktopId = _virtualDesktop.GetCurrentDesktop();
        Screen screen = Screen.FromHandle(_activeWindowHandle);
        Rectangle workingArea = screen.WorkingArea;
        Rectangle zoneArea = screen.WorkingArea with
        {
            Width = workingArea.Width / newZoneCount
        };

        _zones.Clear();
        Window[][] chunks = _windows.Chunk(windowsPerZone).ToArray();
        for (int iChunk = 0; iChunk < chunks.Count(); iChunk++)
        {
            zoneArea.X = iChunk * zoneArea.Width;
            Zone newZone = new()
            {
                DesktopId = desktopId,
                Area = zoneArea
            };

            newZone.AddWindowRange(chunks[iChunk]);
            _zones.Add(newZone);
        }
    }

    /// <summary>
    /// Uses the EnumWindows from user32.dll to get all windows.
    /// Only open windows and windows on the current desktop are returned.
    /// </summary>
    /// <param name="onlyNewWindows">When true, only unmanaged windows will be returned.</param>
    /// <returns>A list of windows</returns>
    private List<Window> GetWindows(bool onlyNewWindows = false)
    {
        List<Window> newWindows = new();

        Guid currentDesktop = _virtualDesktop.GetCurrentDesktop();
        EnumWindows((hwnd, lParam) =>
        {
            Window window = new(hwnd);
            if (!window.Visible ||
                window.Minimized)
            {
                return true;
            }

            if (
                !string.IsNullOrEmpty(window.Title) &&
                ShouldManageWindow(window, currentDesktop) &&
                (!onlyNewWindows || !IsManagedWindow(hwnd))
            )
            {
                newWindows.Add(window);
            }

            return true;
        }, IntPtr.Zero);

        return newWindows;
    }

    /// <summary>
    /// Checks for window handles/titles to ignore and window style
    /// </summary>
    private bool ShouldManageWindow(Window window, Guid currentDesktop)
    {
        string[] ignore = { "Program Manager", "Windows Input Experience", "Task Switching" };

        // Don't manage windows with no title bar or tool windows
        int style = GetWindowLong(window.Handle, GWL_STYLE);
        int exStyle = GetWindowLong(window.Handle, GWL_EXSTYLE);

        // Only handle windows on this desktop
        bool onCurrentDesktop = currentDesktop == _virtualDesktop.GetWindowDesktop(window.Handle);

        bool hasCaption = (style & WS_CAPTION) == WS_CAPTION;
        bool isToolWindow = (exStyle & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW;

        return !ignore.Any(i => window.Title.Contains(i)) && hasCaption && !isToolWindow && onCurrentDesktop;
    }

    /// <summary>
    /// Returns a boolean based on if the window is already present in the window manager
    /// </summary>
    private bool IsManagedWindow(IntPtr hwnd)
    {
        return _windows.Any(w => w.Handle == hwnd);
    }

    public void Dispose()
    {
        _windowEventHook.Dispose();
    }
}