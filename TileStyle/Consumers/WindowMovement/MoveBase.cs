using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public abstract class MoveBase : IKeyConsumer
{
    private readonly ILogger<MoveBase> _logger;
    private readonly WindowManager _windowManager;

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
        if (!activeZone.TryMove(activeWindow, MoveDirection))
        {
            _logger.LogError("Window move failed.");
            // TODO: Move to next zone   
        }

        return Task.CompletedTask;
    }
}