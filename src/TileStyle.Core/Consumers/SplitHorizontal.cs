using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers;

public class SplitHorizontal : IKeyConsumer
{
    private readonly ILogger<SplitHorizontal> _logger;
    private readonly WindowManager _windowManager;
    private readonly WindowStore _windowStore;

    public HotKey HotKey => new(
        Keys.H,
        ModifierKeys.Win
    );

    public SplitHorizontal(WindowManager windowManager, ILogger<SplitHorizontal> logger, WindowStore windowStore)
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

        activeZone.SetLayoutMode(LayoutMode.Horizontal);

        return Task.CompletedTask;
    }
}