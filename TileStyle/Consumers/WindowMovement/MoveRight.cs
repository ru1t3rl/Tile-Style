using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveRight : IKeyConsumer
{
    public KeyCombination HotKey => new(
        Keys.Right,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        return Task.CompletedTask;
    }
}