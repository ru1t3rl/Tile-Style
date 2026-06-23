using Microsoft.Extensions.Logging;
using TileStyle.Models;
using TileStyle.Windows;
using WindowsDesktop;

namespace TileStyle;

public partial class WindowManager : IDisposable
{
    public const int MAX_WINDOWS_PER_ZONE = 2;
    private readonly DwmCornerPreference PREFERED_CORNERS = DwmCornerPreference.DWMWCP_DONOTROUND;

    private readonly ILogger<WindowManager> _logger;
    private readonly WindowEventHook _windowEventHook;
    private readonly VirtualDesktopHelper _virtualDesktop;
    private readonly WindowStyleChanger _styleChanger;

    public readonly Dictionary<Screen, List<Window>> ScreenGroupedWindows = new();
    public readonly List<Zone> Zones = new();
    public readonly List<Window> FloatingWindows = new();

    public List<Window> Windows => ScreenGroupedWindows
        .SelectMany(w => w.Value)
        .Concat(FloatingWindows)
        .ToList();

    private bool _tilingEnabled = true;

    public IntPtr ActiveWindowHandle => GetForegroundWindow();

    public WindowManager(
        WindowEventHook windowEventHook,
        VirtualDesktopHelper virtualDesktop,
        ILogger<WindowManager> logger,
        WindowStyleChanger styleChanger)
    {
        _windowEventHook = windowEventHook;
        _virtualDesktop = virtualDesktop;
        _logger = logger;
        _styleChanger = styleChanger;

        _windowEventHook.WindowCreated += AddNewWindowAsync;
        _windowEventHook.WindowDestroyed += CloseWindowAsync;
        _windowEventHook.WindowShown += AddNewWindowAsync;
        _windowEventHook.WindowMinimized += CloseWindowAsync;
        _windowEventHook.WindowRestored += AddNewWindowAsync;
        _windowEventHook.MouseFocusChanged += OnMouseFocusChanged;
    }

    public async Task InitializeContext()
    {
        await UpdateWindows();
    }

    public async Task ToggleTiling()
    {
        _tilingEnabled = !_tilingEnabled;
        if (!_tilingEnabled)
        {
            await UpdateWindows();
        }
    }

    private void OnMouseFocusChanged(object? sender, WindowEventArgs e)
    {
        if (!IsManagedWindow(e.WindowHandle)) return;
        SetForegroundWindow(e.WindowHandle);
        _logger.LogDebug($"Mouse focus changed to {e.WindowHandle}");
    }

    public async Task AddNewWindowAsync(object? sender, WindowEventArgs e)
    {
        Window window = new Window(e.WindowHandle);
        Guid currentDesktopId = _virtualDesktop.GetCurrentDesktop();

        if (currentDesktopId == Guid.Empty)
        {
            currentDesktopId = await Task.Run(() => VirtualDesktop.FromHwnd(window.Handle)?.Id ?? Guid.Empty);
        }

        _logger.LogDebug("In add zone");

        bool shouldManage = ShouldManageWindow(window, currentDesktopId);
        bool isManagedWindow = IsManagedWindow(window.Handle);

        _logger.LogDebug("SHOULD: {shouldM}, IsMan: {isManag}, IsVisisble: {v}, floating: {f}, minimized: {m}", shouldManage,
            isManagedWindow, window.Visible, window.Floating, window.Minimized);

        _logger.LogDebug("Current D: {d}", currentDesktopId);

        if (
            !window.Visible ||
            window.Floating ||
            window.Minimized ||
            !shouldManage ||
            isManagedWindow ||
            currentDesktopId == Guid.Empty
        )
        {
            return;
        }

        _logger.LogDebug("Doing more");

        int bestDistance = CalculateDistance(Zones[0], window);
        Zone bestZone = Zones[0];
        if (bestZone.Windows.Count >= MAX_WINDOWS_PER_ZONE)
        {
            await UpdateWindows();
            return;
        }

        foreach (Zone zone in Zones)
        {
            int distance = CalculateDistance(zone, window);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestZone = zone;
            }
        }

        UpdateWindowStyle(window.Handle);

        Screen screen = Screen.FromHandle(window.Handle);
        ScreenGroupedWindows[screen].Add(window);
        bestZone.AddWindow(ScreenGroupedWindows[screen][^1]);
    }

    public async Task CloseWindowAsync(object? sender, WindowEventArgs e)
    {
        Window? window = Windows.SingleOrDefault(w => w.Handle == e.WindowHandle);
        if (window is null)
        {
            return;
        }

        Screen screen = Screen.FromHandle(window.Handle);
        ScreenGroupedWindows[screen].Remove(window);

        foreach (Zone zone in Zones)
        {
            if (zone.Windows.All(w => w.Handle != e.WindowHandle))
            {
                continue;
            }

            zone.RemoveWindow(e.WindowHandle);
            if (zone.IsEmpty)
            {
                await UpdateWindows();
                break;
            }
        }
    }

    private int CalculateDistance(Zone zone, Window window)
    {
        Rectangle zoneArea = zone.Area;
        int dX = (int)window.Position.X - zoneArea.X;
        dX *= dX;
        int dY = (int)window.Position.Y - zoneArea.Y;
        dY *= dY;
        return dX + dY;
    }

    /// <summary>
    /// The update windows function performs a full refresh of the entire manager environment.
    /// This will clear/reset the <see cref="Zones"/> and <see cref="ScreenGroupedWindows"/> lists.
    /// </summary>
    private async Task UpdateWindows()
    {
        if (!_tilingEnabled)
        {
            return;
        }

        Dictionary<Screen, List<Window>> windows = (await GetWindows())
            .Select(window =>
            {
                Screen screen = Screen.FromHandle(window.Handle);
                return (screen, window);
            })
            .GroupBy(w => w.screen)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => grouping
                    .Select(group => group.window)
                    .OrderBy(w => w.Position.X)
                    .ToList()
            );

        ScreenGroupedWindows.Clear();

        if (windows.Count == 0)
        {
            return;
        }


        Zones.Clear();
        foreach (Screen screen in windows.Keys)
        {
            List<Window> currentWindows = windows[screen];
            ScreenGroupedWindows.Add(screen, windows[screen]);

            int newZoneCount = (int)Math.Max(1, Math.Ceiling((currentWindows.Count / (float)MAX_WINDOWS_PER_ZONE)));
            int windowsPerZone = (int)Math.Ceiling(currentWindows.Count / (float)newZoneCount);

            Guid desktopId = _virtualDesktop.GetCurrentDesktop();

            if (desktopId == Guid.Empty)
            {
                _logger.LogDebug("Skipping windows, because the desktopId wasn't available.");
                continue;
            }

            Rectangle workingArea = screen.WorkingArea;
            Rectangle zoneArea = screen.WorkingArea with
            {
                Width = workingArea.Width / newZoneCount
            };

            Window[][] chunks = currentWindows.Chunk(windowsPerZone).ToArray();
            for (int iChunk = 0; iChunk < chunks.Count(); iChunk++)
            {
                zoneArea.X = zoneArea.X + iChunk * zoneArea.Width;
                Zone newZone = new()
                {
                    DesktopId = desktopId,
                    Area = zoneArea
                };

                newZone.AddWindowRange(chunks[iChunk]);
                Zones.Add(newZone);
            }
        }
    }

    /// <summary>
    /// Uses the EnumWindows from user32.dll to get all windows.
    /// Only open windows and windows on the current desktop are returned.
    /// </summary>
    /// <param name="onlyNewWindows">When true, only unmanaged windows will be returned.</param>
    /// <returns>A list of windows</returns>
    private async Task<List<Window>> GetWindows(bool onlyNewWindows = false)
    {
        List<IntPtr> newWindowHandles = new();
        IntPtr[] existingWindows = Windows.Select(window => window.Handle).ToArray();

        Guid currentDesktop = _virtualDesktop.GetCurrentDesktop();
        EnumWindows((hwnd, lParam_) =>
        {
            if (!onlyNewWindows || existingWindows.All(h => h != hwnd))
            {
                newWindowHandles.Add(hwnd);
            }

            return true;
        }, IntPtr.Zero);

        List<Window> newWindows = newWindowHandles
            .Select(handle => new Window(handle))
            .Where(window => window.Visible &&
                             !window.Minimized &&
                             !string.IsNullOrEmpty(window.Title) &&
                             ShouldManageWindow(window, currentDesktop) &&
                             (!onlyNewWindows || !IsManagedWindow(window.Handle)))
            .ToList();

        _logger.LogInformation("Found a total of {Count} windows. There are {usable} windows which should be managed.",
            newWindowHandles.Count, newWindows.Count);

        newWindows.ForEach(w => UpdateWindowStyle(w.Handle));

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
        return Windows.Any(w => w.Handle == hwnd);
    }

    private void UpdateWindowStyle(IntPtr handle)
    {
        switch (PREFERED_CORNERS)
        {
            case DwmCornerPreference.DWMWCP_DONOTROUND:
                _styleChanger.SwitchToFullHeightStyle(handle);
                break;
            case DwmCornerPreference.DWMWCP_ROUND:
                _styleChanger.SwitchToFloatingStyle(handle);
                break;
            case DwmCornerPreference.DWMWCP_ROUNDSMALL:
            case DwmCornerPreference.DWMWCP_DEFAULT:
                _styleChanger.SetWindowStyle(handle, PREFERED_CORNERS);
                break;
        }
    }

    public async Task MoveFocusedWindowToDesktop(MoveDirection direction)
    {
        _virtualDesktop.MoveWindowToNextDesktop(ActiveWindowHandle, direction);
        await UpdateWindows();
    }

    public void Dispose()
    {
        _windowEventHook.Dispose();
    }
}