using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveDown : MoveBase
{
    public MoveDown(WindowManager windowManager, ILogger<MoveBase> logger) : base(windowManager, logger)
    {
    }

    public override HotKey HotKey => new HotKey(
        Keys.Down,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    protected override MoveDirection MoveDirection => MoveDirection.Down;
}