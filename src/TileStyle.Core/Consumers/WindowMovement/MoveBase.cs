using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers.WindowMovement;

public abstract class MoveBase : IKeyConsumer
{
    private readonly ILogger<MoveBase> _logger;
    private readonly WindowStore _windowStore;
    protected readonly WindowManager _windowManager;

    public MoveBase(WindowManager windowManager, ILogger<MoveBase> logger, WindowStore windowStore)
    {
        _windowManager = windowManager;
        _logger = logger;
        _windowStore = windowStore;
    }

    public abstract HotKey HotKey { get; }
    protected abstract MoveDirection MoveDirection { get; }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        Window? activeWindow = _windowStore.Windows
            .SingleOrDefault(w => w.Handle == _windowManager.ActiveWindowHandle);

        if (activeWindow is null)
        {
            _logger.LogError("No active window found.");
            return Task.CompletedTask;
        }

        int zoneIndex = _windowStore.Zones.FindIndex(z =>
            z.Windows.Any(w => w.Handle == activeWindow.Handle)
        );

        Zone activeZone = _windowStore.Zones[zoneIndex];
        if (!activeZone.TryMove(activeWindow, MoveDirection) && !TryChangeZone(activeWindow, activeZone))
        {
            _logger.LogError("Window move failed.");
        }

        return Task.CompletedTask;
    }

    private bool TryChangeZone(Window window, Zone zone)
    {
        Zone? nextZone = null;
        int zoneIndex = _windowStore.Zones.IndexOf(zone);
        if (MoveDirection is MoveDirection.Right or MoveDirection.Down)
        {
            if(zoneIndex < _windowStore.Zones.Count - 1)
            {
                nextZone = _windowStore.Zones[zoneIndex + 1];
            }
            else
            {
                nextZone = _windowStore.Zones[0];
            }
        }

        if (MoveDirection is MoveDirection.Left or MoveDirection.Up)
        {
            if(zoneIndex > 0)
            {
                nextZone = _windowStore.Zones[zoneIndex - 1];
            }
            else
            {
                nextZone = _windowStore.Zones[^1];
            }
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