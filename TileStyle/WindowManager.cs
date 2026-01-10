using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms.Design.Behavior;
using TileStyle.Window;

namespace TileStyle;

public partial class WindowManager : IDisposable
{
    private bool _tilingEnabled = true;
    private readonly Dictionary<Guid, List<IntPtr>> _desktopWindows = new Dictionary<Guid, List<IntPtr>>();
    private readonly HashSet<IntPtr> _floatingWindows = new HashSet<IntPtr>();
    private IntPtr _activeWindow;
    private LayoutMode _mode = LayoutMode.Horizontal;
    private readonly VirtualDesktopHelper _desktopHelper;
    private readonly WindowEventHook _eventHook;
    private readonly SynchronizationContext? _syncContext;

    public enum LayoutMode
    {
        Horizontal,
        Vertical
    }

    public WindowManager(VirtualDesktopHelper desktopHelper, WindowEventHook eventHook)
    {
        _syncContext = SynchronizationContext.Current;
        _desktopHelper = desktopHelper;
        _eventHook = eventHook;

        _eventHook.WindowCreated += OnWindowEvent;
        _eventHook.WindowDestroyed += OnWindowEvent;
        _eventHook.WindowShown += OnWindowEvent;
        _eventHook.WindowMinimized += OnWindowEvent;
        _eventHook.WindowRestored += OnWindowEvent;

        RefreshWindows();
    }

    private void OnWindowEvent(object? sender, WindowEventArgs e)
    {
        // Delay slightly to let window state stabilize
        Task.Delay(50).ContinueWith(_ => { _syncContext?.Post(_ => RefreshWindows(), null); });
    }

    public void RefreshWindows()
    {
        UpdateWindows();
    }

    public void ToggleTiling()
    {
        _tilingEnabled = !_tilingEnabled;
        if (_tilingEnabled) TileWindows();
        else RestoreWindows();
    }

    public void ToggleFloating()
    {
        IntPtr hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return;

        if (_floatingWindows.Contains(hwnd))
        {
            _floatingWindows.Remove(hwnd);
        }
        else
        {
            _floatingWindows.Add(hwnd);
        }

        RefreshWindows();
    }

    public void MoveWindowInGrid(int deltaX, int deltaY)
    {
        Guid currentDesktop = _desktopHelper.GetCurrentDesktop();
        if (!_desktopWindows.ContainsKey(currentDesktop)) return;

        var windows = _desktopWindows[currentDesktop];
        IntPtr hwnd = GetForegroundWindow();

        int currentIndex = windows.IndexOf(hwnd);
        if (currentIndex == -1) return;

        int newIndex;

        if (_mode == LayoutMode.Horizontal)
        {
            // In horizontal mode, left/right moves in the list
            newIndex = currentIndex + deltaX;
        }
        else
        {
            // In vertical mode, up/down moves in the list
            newIndex = currentIndex + deltaY;
        }

        // Clamp to valid range
        newIndex = Math.Max(0, Math.Min(windows.Count - 1, newIndex));

        if (newIndex != currentIndex)
        {
            // Swap windows in the list
            var temp = windows[currentIndex];
            windows[currentIndex] = windows[newIndex];
            windows[newIndex] = temp;

            TileWindows();
            SetForegroundWindow(hwnd);
        }
    }

    public void MoveWindowToDesktop(int direction)
    {
        IntPtr hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return;

        try
        {
            // Get all available desktops
            var desktops = GetAllDesktops();
            Guid currentDesktop = _desktopHelper.GetCurrentDesktop();

            int currentIndex = Array.IndexOf(desktops, currentDesktop);
            if (currentIndex == -1) return;

            int newIndex = currentIndex + direction;
            if (newIndex < 0 || newIndex >= desktops.Length) return;

            Guid targetDesktop = desktops[newIndex];
            _desktopHelper.MoveWindowToDesktop(hwnd, targetDesktop);

            // Switch to the target desktop
            SwitchToDesktop(newIndex + 1);

            RefreshWindows();
        }
        catch
        {
            // Desktop switching not supported or failed
        }
    }

    private Guid[] GetAllDesktops()
    {
        // This is a simplified approach - we'll track desktops we've seen
        return _desktopWindows.Keys.ToArray();
    }

    private void SwitchToDesktop(int desktopNumber)
    {
        // Simulate Win+Ctrl+Left/Right to switch desktops
        // This is a workaround since there's no direct API
    }

    public void SplitHorizontal()
    {
        _mode = LayoutMode.Horizontal;
        TileWindows();
    }

    public void SplitVertical()
    {
        _mode = LayoutMode.Vertical;
        TileWindows();
    }

    public void FocusNext()
    {
        Guid currentDesktop = _desktopHelper.GetCurrentDesktop();
        if (!_desktopWindows.ContainsKey(currentDesktop) || _desktopWindows[currentDesktop].Count == 0) return;

        var windows = _desktopWindows[currentDesktop];
        int idx = windows.IndexOf(_activeWindow);
        idx = (idx + 1) % windows.Count;
        SetForegroundWindow(windows[idx]);
        _activeWindow = windows[idx];
    }

    public void FocusPrevious()
    {
        Guid currentDesktop = _desktopHelper.GetCurrentDesktop();
        if (!_desktopWindows.ContainsKey(currentDesktop) || _desktopWindows[currentDesktop].Count == 0) return;

        var windows = _desktopWindows[currentDesktop];
        int idx = windows.IndexOf(_activeWindow);
        idx = (idx - 1 + windows.Count) % windows.Count;
        SetForegroundWindow(windows[idx]);
        _activeWindow = windows[idx];
    }

    public void CloseActiveWindow()
    {
        if (_activeWindow != IntPtr.Zero)
        {
            SendMessage(_activeWindow, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }

    private void UpdateWindows()
    {
        _desktopWindows.Clear();
        Guid currentDesktop = _desktopHelper.GetCurrentDesktop();

        EnumWindows((hwnd, lParam) =>
        {
            if (IsWindowVisible(hwnd) && !IsIconic(hwnd))
            {
                string title = GetWindowTitle(hwnd);
                if (!string.IsNullOrEmpty(title) && ShouldManageWindow(hwnd) && !_floatingWindows.Contains(hwnd))
                {
                    Guid desktop = _desktopHelper.GetWindowDesktop(hwnd);
                    if (desktop != Guid.Empty)
                    {
                        if (!_desktopWindows.ContainsKey(desktop))
                        {
                            _desktopWindows[desktop] = new List<IntPtr>();
                        }

                        _desktopWindows[desktop].Add(hwnd);
                    }
                }
            }

            return true;
        }, IntPtr.Zero);

        if (_tilingEnabled) TileWindows();
    }

    private string GetWindowTitle(IntPtr hwnd)
    {
        int length = GetWindowTextLength(hwnd);
        if (length == 0) return string.Empty;

        StringBuilder sb = new StringBuilder(length + 1);
        GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private bool ShouldManageWindow(IntPtr hwnd)
    {
        string title = GetWindowTitle(hwnd);
        string[] ignore = { "Program Manager", "Windows Input Experience", "Task Switching" };

        // Don't manage windows with no title bar or tool windows
        int style = GetWindowLong(hwnd, GWL_STYLE);
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        bool hasCaption = (style & WS_CAPTION) == WS_CAPTION;
        bool isToolWindow = (exStyle & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW;

        return !ignore.Any(i => title.Contains(i)) && hasCaption && !isToolWindow;
    }

    private void TileWindows()
    {
        if (!_tilingEnabled) return;

        Guid currentDesktop = _desktopHelper.GetCurrentDesktop();
        if (!_desktopWindows.ContainsKey(currentDesktop)) return;

        var windows = _desktopWindows[currentDesktop];
        if (windows.Count == 0) return;

        Screen screen = Screen.PrimaryScreen;
        Rectangle workArea = screen.WorkingArea;

        int count = windows.Count;

        for (int i = 0; i < count; i++)
        {
            Rectangle rect;

            if (_mode == LayoutMode.Horizontal)
            {
                int width = workArea.Width / count;
                rect = new Rectangle(
                    workArea.X + i * width,
                    workArea.Y,
                    width,
                    workArea.Height
                );
            }
            else
            {
                int height = workArea.Height / count;
                rect = new Rectangle(
                    workArea.X,
                    workArea.Y + i * height,
                    workArea.Width,
                    height
                );
            }

            SetWindowPos(windows[i], IntPtr.Zero,
                rect.X, rect.Y, rect.Width, rect.Height,
                SWP_NOZORDER | SWP_NOACTIVATE);
        }
    }

    private void RestoreWindows()
    {
        // Windows will restore to their previous positions naturally
    }

    public void Dispose()
    {
        _eventHook?.Dispose();
    }
}