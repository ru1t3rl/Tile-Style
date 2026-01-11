using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveLeft : IKeyConsumer
{
    public HotKey HotKey => new(
        Keys.Left,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        return Task.CompletedTask;
    }
}