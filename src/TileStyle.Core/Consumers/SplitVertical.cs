using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers;

public class SplitVertical : IKeyConsumer
{
    private readonly ILogger<SplitVertical> _logger;
    private readonly WindowManager _windowManager;
    private readonly WindowStore _windowStore;

    public HotKey HotKey => new(
        Keys.V,
        ModifierKeys.Win
    );

    public SplitVertical(WindowManager windowManager, ILogger<SplitVertical> logger, WindowStore windowStore)
    {
        _windowManager = windowManager;
        _logger = logger;
        _windowStore = windowStore;
    }

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        Zone? activeZone = _windowStore.Zones
            .SingleOrDefault(z => z.Windows.Any(w => w.Handle == _windowManager.ActiveWindowHandle));

        if (activeZone is null)
        {
            _logger.LogWarning("Active zone not found");
            return Task.CompletedTask;
        }

        activeZone.SetLayoutMode(LayoutMode.Vertical);

        return Task.CompletedTask;
    }
}