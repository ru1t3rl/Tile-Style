using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers.WindowMovement;

public class MoveRight : MoveBase
{
    public override HotKey HotKey => new HotKey(
        Keys.Right,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    protected override MoveDirection MoveDirection => MoveDirection.Right;

    public MoveRight(WindowManager windowManager, ILogger<MoveBase> logger, WindowStore store) 
        : base(windowManager, logger, store)
    {
    }
}