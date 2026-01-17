using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;

namespace TileStyle.Consumers.WindowMovement;

public class MoveUp : MoveBase
{
    public MoveUp(WindowManager windowManager, ILogger<MoveBase> logger) : base(windowManager, logger)
    {
    }

    public override HotKey HotKey => new HotKey(
        Keys.Up,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    protected override MoveDirection MoveDirection => MoveDirection.Up;
}