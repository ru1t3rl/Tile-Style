using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public abstract class MoveBase : IKeyConsumer
{
    private readonly ILogger<MoveBase> _logger;
    protected readonly WindowManager _windowManager;

    public MoveBase(WindowManager windowManager, ILogger<MoveBase> logger)
    {
        _windowManager = windowManager;
        _logger = logger;
    }

    public abstract HotKey HotKey { get; }
    protected abstract MoveDirection MoveDirection { get; }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        Window? activeWindow = _windowManager.Windows
            .SingleOrDefault(w => w.Handle == _windowManager.ActiveWindowHandle);

        if (activeWindow is null)
        {
            _logger.LogError("No active window found.");
            return Task.CompletedTask;
        }

        int zoneIndex = _windowManager.Zones.FindIndex(z =>
            z.Windows.Any(w => w.Handle == activeWindow.Handle)
        );

        Zone activeZone = _windowManager.Zones[zoneIndex];
        if (!activeZone.TryMove(activeWindow, MoveDirection) && !TryChangeZone(activeWindow, activeZone))
        {
            _logger.LogError("Window move failed.");
        }

        return Task.CompletedTask;
    }

    private bool TryChangeZone(Window window, Zone zone)
    {
        Zone? nextZone = null;
        int zoneIndex = _windowManager.Zones.IndexOf(zone);
        if ((MoveDirection == MoveDirection.Right || MoveDirection == MoveDirection.Down) &&
            zoneIndex < _windowManager.Zones.Count - 1)
        {
            nextZone = _windowManager.Zones[zoneIndex + 1];
        }

        if ((MoveDirection == MoveDirection.Left || MoveDirection == MoveDirection.Up) && zoneIndex > 0)
        {
            nextZone = _windowManager.Zones[zoneIndex - 1];
        }

        if (nextZone is null)
        {
            return false;
        }

        zone.RemoveWindow(window.Handle);
        nextZone.AddWindow(window);

        return true;
    }
}