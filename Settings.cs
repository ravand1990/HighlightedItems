using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using System.Windows.Forms;

namespace HighlightedItems
{
  public class Settings : ISettings
  {
    public int[,] IgnoredCells { get; set; } = new int[5, 12]
    {
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      },
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      },
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      },
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      },
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      }
    };

    public ToggleNode Enable { get; set; } = new ToggleNode(true);

    [Menu("Enable Inventory Dump Button")]
    public ToggleNode DumpButtonEnable { get; set; } = new ToggleNode(true);

    [Menu("Show Stack Sizes")]
    public ToggleNode ShowStackSizes { get; set; } = new ToggleNode(true);

    [Menu("Show Stack Count Next to Stack Size")]
    public ToggleNode ShowStackCountWithSize { get; set; } = new ToggleNode(true);

    [Menu("Hotkey")]
    public HotkeyNode HotKey { get; set; } = new HotkeyNode(Keys.F1);

    [Menu("ExtraDelay")]
    public RangeNode<int> ExtraDelay { get; set; } = new RangeNode<int>(20, 0, 100);

    [Menu("Use Thread.Sleep", "Is a little faster, but HUD will hang while clicking")]
    public ToggleNode UseThreadSleep { get; set; } = new ToggleNode(false);

    [Menu("Idle mouse delay", "Wait this long after the user lets go of the button and stops moving the mouse")]
    public RangeNode<int> IdleMouseDelay { get; set; } = new RangeNode<int>(200, 0, 1000);

    public ToggleNode CancelWithRightMouseButton { get; set; } = new ToggleNode(true);
  }
}
