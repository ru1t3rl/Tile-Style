using TileStyle.Models;

namespace TileStyle.Windows;

public class WindowStore
{
    public Dictionary<Screen, List<Window>> ScreenGroupedWindows { get; init; } = new();
    public List<Zone> Zones { get; init; } = new();
    public List<Window> FloatingWindows { get; init; } = new();

    public Dictionary<Guid, List<Window>> DesktopGroupedWindows => Zones
        .GroupBy(zone => zone.DesktopId)
        .ToDictionary(
            group => group.Key,
            group => group.SelectMany(zone => zone.Windows).ToList()
        );

    public List<Window> Windows => ScreenGroupedWindows
        .SelectMany(w => w.Value)
        .Concat(FloatingWindows)
        .ToList();
}