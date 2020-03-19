using System.Collections.Generic;
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
            this.IgnoredCells = new int[5, 12] {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}
            };
        }

        public int[,] IgnoredCells { get; set; }


        [Menu("Enable")]
        public ToggleNode Enable { get; set; }

        [Menu("Hotkey")]
        public HotkeyNode HotKey { get; set; }

        [Menu("Speed")]
        public RangeNode<int> Speed { get; set; }

        public Dictionary<string, StashTabNode> FilterOptions = new Dictionary<string, StashTabNode>();

        public StashTabNode CurrencyStashTab { get; set; } = new StashTabNode();




    }
}