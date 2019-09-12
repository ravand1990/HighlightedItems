using System.Windows.Forms;
using HighlightedItems.Utils;
using System.Threading;
using ExileCore;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace HighlightedItems
{
    public class HighlightedItems : BaseSettingsPlugin<Settings>
    {
        private IngameState ingameState;
        private SharpDX.Vector2 windowOffset = new SharpDX.Vector2();
           
        public HighlightedItems()
        {
        }

        public override bool Initialise()
        {
            base.Initialise();
            Name = "HighlightedItems";
            ingameState = GameController.Game.IngameState;
            windowOffset = GameController.Window.GetWindowRectangle().TopLeft;

            var combine = Path.Combine(DirectoryFullName, "images\\pick.png").Replace('\\', '/');
            Graphics.InitImage(combine, false);

            return true;
        }

        public override void Render()
        {
            if (!Settings.Enable)
                return;

            var stashPanel = ingameState.IngameUi.StashElement;
            if (stashPanel.IsVisible)
            {
                var visibleStash = stashPanel.VisibleStash;
                if (visibleStash == null)
                    return;
                var stashRect = visibleStash.InventoryUIElement.GetClientRect();
                var pickButtonRect = new SharpDX.RectangleF(stashRect.BottomRight.X - 43, stashRect.BottomRight.Y + 10, 37, 37);

                Graphics.DrawImage("pick.png", pickButtonRect);

                var highlightedItems = GetHighlightedItems();
            
                var countText = highlightedItems.Count.ToString();
                var  countPos = pickButtonRect.Center;
                var countDigits = highlightedItems.Count.ToString().Length;
                SizeF size = TextRenderer.MeasureText(countText, new Font("Arial", 20)); 
                countPos.X -= size.Width;
                countPos.Y -= 11;

                Graphics.DrawText(countText, countPos, SharpDX.Color.White);

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
                if (Keyboard.IsKeyPressed(Settings.HotKey.Value))
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
                var inventoryItems = ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems;
                foreach (var item in inventoryItems)
                {
                    bool isHighlighted = item.isHighlighted;
                    if (isHighlighted)
                    {
                        highlightedItems.Add(item);
                    }
                }
            }
            catch (System.Exception){}
            return highlightedItems;
        }

        public void moveItem(SharpDX.Vector2 itemPosition)
        {
            itemPosition += windowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            Thread.Sleep(Mouse.DELAY_MOVE);
            Mouse.moveMouse(itemPosition);
            Mouse.LeftUp(Settings.Speed);
            Thread.Sleep(Mouse.DELAY_MOVE);
            Keyboard.KeyUp(Keys.LControlKey);
        }
    }
}