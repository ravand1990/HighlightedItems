using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.RemoteMemoryObjects;
using System.Windows.Forms;
using HighlightedItems.Utils;
using System.Threading;
using SharpDX;
using PoeHUD.Framework;
using System.Collections.Generic;
using PoeHUD.Poe.Elements;
using System.Drawing;

namespace HighlightedItems
{
    internal class HighlightedItems : BaseSettingsPlugin<Settings>
    {
        private IngameState ingameState;
        private Vector2 windowOffset = new Vector2();


        public HighlightedItems()
        {
            PluginName = "HighlightedItems";
        }

        public override void Initialise()
        {
            ingameState = GameController.Game.IngameState;
            windowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            base.Initialise();
        }

        public override void Render()
        {
            if (!Settings.Enable)
                return;

            var stashPanel = ingameState.ServerData.StashPanel;
            if (stashPanel.IsVisible)
            {
                var visibleStash = stashPanel.VisibleStash;
                if (visibleStash == null)
                    return;
                var stashRect = visibleStash.InventoryUiElement.GetClientRect();
                var pickButtonRect = new SharpDX.RectangleF(stashRect.BottomRight.X - 43, stashRect.BottomRight.Y + 10, 37, 37);

                Graphics.DrawPluginImage(PluginDirectory + "\\images\\pick.png", pickButtonRect);

                var highlightedItems = GetHighlightedItems();
            
                var countText = highlightedItems.Count.ToString();
                var countPos = pickButtonRect.Center;
                var countDigits = highlightedItems.Count.ToString().Length;
                SizeF size = TextRenderer.MeasureText(countText, new Font("Arial", 20)); 
                countPos.X -= size.Width;
                countPos.Y -= 11;

                Graphics.DrawText(countText, 20, countPos);

                if (Control.MouseButtons == MouseButtons.Left)
                {
                    var prevMousePos = Mouse.GetCursorPosition();

                    if (pickButtonRect.Contains(Mouse.GetCursorPosition() - windowOffset)) { 
                        foreach (var item in highlightedItems)
                        {
                            moveItem(item.GetClientRect().Center);
                        }
                    }
                    Mouse.moveMouse(prevMousePos);
                }
                if (Settings.HotKey.PressedOnce())
                {
                    var prevMousePos = Mouse.GetCursorPosition();
                    foreach (var item in highlightedItems)
                    {
                        moveItem(item.GetClientRect().Center);
                    }
                    Mouse.moveMouse(prevMousePos);
                }
            }
        }

        public List<NormalInventoryItem> GetHighlightedItems()
        {
            List<NormalInventoryItem> highlightedItems = new List<NormalInventoryItem>();
            try
            {
                var inventoryItems = ingameState.ServerData.StashPanel.VisibleStash.VisibleInventoryItems;
                foreach (var item in inventoryItems)
                {
                    bool isHighlighted = item.IsHighlighted;
                    if (isHighlighted)
                    {
                        highlightedItems.Add(item);
                    }
                }
            }
            catch (System.Exception){}
            return highlightedItems;
        }



        public void moveItem(Vector2 itemPosition)
        {
            itemPosition += windowOffset;
            Keyboard.HoldKey((byte)Keys.LControlKey);
            Thread.Sleep(Mouse.DELAY_MOVE);
            Mouse.moveMouse(itemPosition);
            Mouse.LeftUp(Settings.Speed);
            Thread.Sleep(Mouse.DELAY_MOVE);
            Keyboard.ReleaseKey((byte)Keys.LControlKey);
        }
    }
}