using System.Windows.Forms;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace HighlightedItems
{
    internal class Settings : SettingsBase
    {
        public Settings()
        {
            Enable = true;
            HotKey = Keys.F7;
            Speed = new RangeNode<int>(20, 0, 100);
        }

        [Menu("Hotkey")]
        public HotkeyNode HotKey { get; set; }

        [Menu("Speed")]
        public RangeNode<int> Speed { get; set; }
    }
}