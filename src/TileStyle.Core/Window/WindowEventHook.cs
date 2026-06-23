using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Threading;

namespace TileStyle.Windows;

public partial class WindowEventHook : IDisposable
{
    private readonly ILogger<WindowEventHook> _logger;
    private IntPtr[] _hookHandles = new IntPtr[3];
    private IntPtr _mouseHookHandle;

    public event AsyncEventHandler<WindowEventArgs>? WindowCreated;
    public event AsyncEventHandler<WindowEventArgs>? WindowDestroyed;
    public event AsyncEventHandler<WindowEventArgs>? WindowShown;
    public event AsyncEventHandler<WindowEventArgs>? WindowMinimized;
    public event AsyncEventHandler<WindowEventArgs>? WindowRestored;
    public event EventHandler<WindowEventArgs>? MouseFocusChanged;

    private readonly WinEventDelegate _hookDelegate;
    private readonly MouseProc _mouseDelegate;
    private IntPtr _lastMouseHwnd;

    public WindowEventHook(ILogger<WindowEventHook> logger)
    {
        _logger = logger;

        _hookDelegate = new(WindowEventProcessor);
        _mouseDelegate = new(MouseHookCallback);

        _hookHandles[0] = SetWinEventHook(
            EVENT_OBJECT_CREATE, EVENT_OBJECT_DESTROY,
            IntPtr.Zero, _hookDelegate, 0, 0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

        _hookHandles[1] = SetWinEventHook(
            EVENT_OBJECT_SHOW, EVENT_OBJECT_HIDE,
            IntPtr.Zero, _hookDelegate, 0, 0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

        _hookHandles[2] = SetWinEventHook(
            EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZEEND,
            IntPtr.Zero, _hookDelegate, 0, 0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

        _mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL, _mouseDelegate, IntPtr.Zero, 0);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WM_MOUSEMOVE)
        {
            var data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var hwnd = GetAncestor(WindowFromPoint(data.pt), GA_ROOT);

            if (hwnd != IntPtr.Zero && hwnd != _lastMouseHwnd)
            {
                _lastMouseHwnd = hwnd;
                _logger.LogDebug("Mouse focus changed to {hwnd}.", hwnd);
                MouseFocusChanged?.Invoke(this, new WindowEventArgs(hwnd));
            }
        }

        return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
    }

    private void WindowEventProcessor(
        IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
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
        for (int iHook = 0; iHook < _hookHandles.Length; iHook++)
        {
            UnhookWinEvent(_hookHandles[iHook]);
            _logger.LogDebug("Unhooked WinEvent Hook. ({handle})", _hookHandles[iHook]);
            _hookHandles[iHook] = IntPtr.Zero;
        }

        UnhookWindowsHookEx(_mouseHookHandle);
        _mouseHookHandle = IntPtr.Zero;
    }
}