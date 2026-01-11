using System.Runtime.InteropServices;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers;

public class SwitchDesktopRight : IKeyConsumer
{

    public HotKey HotKey => new(
        Keys.Left,
        ModifierKeys.Win
    );

    public Task ExecuteAsync(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    
    
}