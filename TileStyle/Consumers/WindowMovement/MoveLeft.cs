using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveLeft : MoveBase
{
    public MoveLeft(WindowManager windowManager, ILogger<MoveBase> logger) : base(windowManager, logger)
    {
    }

    public override HotKey HotKey => new HotKey(
        Keys.Left,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    protected override MoveDirection MoveDirection => MoveDirection.Left;
}