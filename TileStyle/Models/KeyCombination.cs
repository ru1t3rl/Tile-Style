using TileStyle.Keyboard;

namespace TileStyle.Models;

public record struct KeyCombination(
    Keys MainKey,
    ModifierKeys ModifierKeys
);