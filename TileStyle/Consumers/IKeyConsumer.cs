using TileStyle.Models;

namespace TileStyle.Consumers;

public interface IKeyConsumer
{
    HotKey HotKey { get; }
    Task ExecuteAsync(object? sender, EventArgs e);
}