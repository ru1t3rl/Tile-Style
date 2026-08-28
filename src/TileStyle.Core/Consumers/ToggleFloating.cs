using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;
using WindowsDesktop;

namespace TileStyle.Consumers;

public class ToggleFloating : IKeyConsumer
{
    private readonly ILogger<ToggleFloating> _logger;
    private readonly WindowManager _windowManager;
    private readonly WindowStore _windowStore;

    public HotKey HotKey => new(
        Keys.F,
        ModifierKeys.Win
    );

    public ToggleFloating(WindowManager windowManager, ILogger<ToggleFloating> logger, WindowStore windowStore)
    {
        _windowManager = windowManager;
        _logger = logger;
        _windowStore = windowStore;
    }

    public async Task ExecuteAsync(object? sender, EventArgs e)
    {
        Window? window = _windowStore.Windows.SingleOrDefault(w => w.Handle == _windowManager.ActiveWindowHandle);
        if (window is null)
        {
            _logger.LogWarning("Window {WindowHandle} was not found", _windowManager.ActiveWindowHandle);
            return;
        }

        window.Floating = !window.Floating;

        if (window.Floating)
        {
            _windowStore.FloatingWindows.Add(window);

            Screen screen = Screen.FromHandle(window.Handle);
            _windowStore.ScreenGroupedWindows[screen].Remove(window);

            Zone zone = _windowStore.Zones.Single(z => z.Windows.Any(w => w.Handle == window.Handle));
            zone.RemoveWindow(window.Handle);

            VirtualDesktop.UnpinWindow(window.Handle);

            return;
        }

        _windowStore.FloatingWindows.RemoveAll(w => w.Handle == window.Handle);
        await _windowManager.AddNewWindowAsync(this, new WindowEventArgs(window.Handle));
        VirtualDesktop.PinWindow(window.Handle);
    }
}