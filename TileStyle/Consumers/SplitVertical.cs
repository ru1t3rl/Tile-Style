using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SplitVertical : IKeyConsumer
{
    private readonly ILogger<SplitVertical> _logger;
    private readonly WindowManager _windowManager;

    public HotKey HotKey => new(
        Keys.V,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    public SplitVertical(WindowManager windowManager, ILogger<SplitVertical> logger)
    {
        _windowManager = windowManager;
        _logger = logger;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        Zone? activeZone =
            _windowManager.Zones.SingleOrDefault(z => z.Windows.Any(w => w.Handle == _windowManager.ActiveWindowHandle));

        if (activeZone is null)
        {
            _logger.LogWarning("Active zone not found");
            return Task.CompletedTask;
        }

        activeZone.SetLayoutMode(LayoutMode.Vertical);

        return Task.CompletedTask;
    }
}