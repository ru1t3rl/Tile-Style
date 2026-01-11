using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveDown : IKeyConsumer
{
    public HotKey HotKey => new(
        Keys.Down,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        return Task.CompletedTask;
    }
}