using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers;

public class ToggleFloating : IKeyConsumer
{
    private readonly ILogger<ToggleFloating> _logger;
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.F,
        ModifierKeys.Win
    );

    public ToggleFloating(WindowManager windowManager, ILogger<ToggleFloating> logger)
    {
        _windowManager = windowManager;
        _logger = logger;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        Window? window = _windowManager.Windows.SingleOrDefault(w => w.Handle == _windowManager.ActiveWindowHandle);
        if (window is null)
        {
            _logger.LogWarning("Window {WindowHandle} was not found", _windowManager.ActiveWindowHandle);
            return Task.CompletedTask;
        }

        if (window.Floating)
        {
            Screen screen = Screen.FromHandle(window.Handle);
            _windowManager.ScreenGroupedWindows[screen].Remove(window);
            _windowManager.AddNewWindow(this, new WindowEventArgs(window.Handle));
            return Task.CompletedTask;
        }

        Zone zone = _windowManager.Zones.Single(z => z.Windows.Any(w => w.Handle == window.Handle));
        zone.RemoveWindow(window.Handle);
        window.Floating = !window.Floating;
        return Task.CompletedTask;
    }
}