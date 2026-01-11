using TileStyle.Keyboard;

namespace TileStyle.Models;

public record struct HotKey(
    Keys MainKey,
    ModifierKeys ModifierKeys
);