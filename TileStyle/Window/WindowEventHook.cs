namespace TileStyle.Window;

public partial class WindowEventHook : IDisposable
{
    private IntPtr _hookHandle;

    public event EventHandler<WindowEventArgs>? WindowCreated;
    public event EventHandler<WindowEventArgs>? WindowDestroyed;
    public event EventHandler<WindowEventArgs>? WindowShown;
    public event EventHandler<WindowEventArgs>? WindowMinimized;
    public event EventHandler<WindowEventArgs>? WindowRestored;

    public WindowEventHook()
    {
        WinEventDelegate hookDelegate = new(WindowEventProcessor);

        _hookHandle = SetWinEventHook(
            EVENT_OBJECT_CREATE,
            EVENT_OBJECT_DESTROY,
            IntPtr.Zero,
            hookDelegate,
            0,
            0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS
        );

        SetWinEventHook(
            EVENT_OBJECT_SHOW,
            EVENT_OBJECT_HIDE,
            IntPtr.Zero,
            hookDelegate,
            0,
            0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS
        );
    }

    private void WindowEventProcessor(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime)
    {
        if (idObject != 0 || idChild != 0) return;
        if (hwnd == IntPtr.Zero) return;

        var args = new WindowEventArgs(hwnd);

        switch (eventType)
        {
            case EVENT_OBJECT_CREATE:
                WindowCreated?.Invoke(this, args);
                break;
            case EVENT_OBJECT_DESTROY:
                WindowDestroyed?.Invoke(this, args);
                break;
            case EVENT_OBJECT_SHOW:
                WindowShown?.Invoke(this, args);
                break;
            case EVENT_OBJECT_HIDE:
                WindowMinimized?.Invoke(this, args);
                break;
            case EVENT_SYSTEM_MINIMIZESTART:
                WindowMinimized?.Invoke(this, args);
                break;
            case EVENT_SYSTEM_MINIMIZEEND:
                WindowRestored?.Invoke(this, args);
                break;
        }
    }

    public void Dispose()
    {
        if (_hookHandle == IntPtr.Zero)
        {
            return;
        }

        UnhookWinEvent(_hookHandle);
        _hookHandle = IntPtr.Zero;
    }
}