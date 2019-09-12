using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;


namespace HighlightedItems
{
    public class Settings : ISettings
    {
        public Settings()
        {
            Enable = new ToggleNode(true);
            HotKey = new HotkeyNode(Keys.F1);
            Speed = new RangeNode<int>(20, 0, 100);
        }

        [Menu("Enable")]
        public ToggleNode Enable { get; set; }

        [Menu("Hotkey")]
        public HotkeyNode HotKey { get; set; }

        [Menu("Speed")]
        public RangeNode<int> Speed { get; set; }
    }
}