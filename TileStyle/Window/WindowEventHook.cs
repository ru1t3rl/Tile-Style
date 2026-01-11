using Microsoft.Extensions.Logging;

namespace TileStyle.Windows;

public partial class WindowEventHook : IDisposable
{
    private readonly ILogger<WindowEventHook> _logger;
    private IntPtr _hookHandle;

    public event EventHandler<WindowEventArgs>? WindowCreated;
    public event EventHandler<WindowEventArgs>? WindowDestroyed;
    public event EventHandler<WindowEventArgs>? WindowShown;
    public event EventHandler<WindowEventArgs>? WindowMinimized;
    public event EventHandler<WindowEventArgs>? WindowRestored;


    public WindowEventHook(ILogger<WindowEventHook> logger)
    {
        _logger = logger;
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
                _logger.LogDebug("Triggered Event Object Create ({eventType}).", eventType);
                WindowCreated?.Invoke(this, args);
                break;
            case EVENT_OBJECT_DESTROY:
                _logger.LogDebug("Triggered Event Object Destroy ({eventType}).", eventType);
                WindowDestroyed?.Invoke(this, args);
                break;
            case EVENT_OBJECT_SHOW:
                _logger.LogDebug("Triggered Event Object Show ({eventType}).", eventType);
                WindowShown?.Invoke(this, args);
                break;
            case EVENT_OBJECT_HIDE:
                _logger.LogDebug("Triggered Event Object Hide ({eventType}).", eventType);
                WindowMinimized?.Invoke(this, args);
                break;
            case EVENT_SYSTEM_MINIMIZESTART:
                _logger.LogDebug("Triggered Event Minimize Start ({eventType}).", eventType);
                WindowMinimized?.Invoke(this, args);
                break;
            case EVENT_SYSTEM_MINIMIZEEND:
                _logger.LogDebug("Triggered Event Minimize End ({eventType}).", eventType);
                WindowRestored?.Invoke(this, args);
                break;
            default:
                _logger.LogWarning("Unhandled event type {eventType}.", eventType);
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