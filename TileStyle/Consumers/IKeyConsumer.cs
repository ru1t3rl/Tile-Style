using TileStyle.Models;

namespace TileStyle.Consumers;

public interface IKeyConsumer
{
    KeyCombination HotKey { get; }
    Task ExecuteAsync(object? sender, EventArgs e);
}