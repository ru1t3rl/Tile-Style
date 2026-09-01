using Microsoft.Extensions.Logging;
using TileStyle.Keyboard;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Consumers.WindowMovement;

public class MoveLeft : MoveBase
{
    public MoveLeft(WindowManager windowManager, ILogger<MoveBase> logger, WindowStore store)
        : base(windowManager, logger, store)
    {
    }

    public override HotKey HotKey => new HotKey(
        Keys.Left,
        ModifierKeys.Win | ModifierKeys.Control | ModifierKeys.Shift
    );

    protected override MoveDirection MoveDirection => MoveDirection.Left;
}