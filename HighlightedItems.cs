using System.Windows.Forms;
using HighlightedItems.Utils;
using ExileCore;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ExileCore.Shared.Enums;
using ImGuiNET;
using System;
using System.Numerics;

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

            var pickBtn = Path.Combine(DirectoryFullName, "images\\pick.png").Replace('\\', '/');
            var pickLBtn = Path.Combine(DirectoryFullName, "images\\pickL.png").Replace('\\', '/');
            Graphics.InitImage(pickBtn, false);
            Graphics.InitImage(pickLBtn, false);

            return true;
        }

        public override void DrawSettings()
        {
            base.DrawSettings();
            this.DrawIgnoredCellsSettings();
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

                //Determine Stash Pickup Button position and draw
                var stashRect = visibleStash.InventoryUIElement.GetClientRect();
                var pickButtonRect =
                    new SharpDX.RectangleF(stashRect.BottomRight.X - 43, stashRect.BottomRight.Y + 10, 37, 37);

                Graphics.DrawImage("pick.png", pickButtonRect);

                var highlightedItems = GetHighlightedItems();


                var count = 0;

                foreach (var item in highlightedItems)
                {
                    count = item.Children[0] != null ? count + Int32.Parse(item.Children[0].Text) : 1;
                }

                var countText = count.ToString();
                var countPos = pickButtonRect.Center;
                var countDigits = highlightedItems.Count.ToString().Length;
                SizeF size = TextRenderer.MeasureText(countText, new Font("Arial", 20));
                countPos.X -= size.Width;
                countPos.Y -= 11;

                Graphics.DrawText(countText, countPos, SharpDX.Color.White);

                if (isButtonPressed(pickButtonRect))
                {
                    var prevMousePos = Mouse.GetCursorPosition();

                    foreach (var item in highlightedItems)
                    {
                        moveItem(item.GetClientRect().Center);
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

            var inventoryPanel = ingameState.IngameUi.InventoryPanel;
            var inventoryItems = inventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            if (inventoryPanel.IsVisible)
            {
                //Determine Inventory Pickup Button position and draw
                var inventoryRect = inventoryPanel.Children[2].GetClientRect();
                var pickButtonRect =
                    new SharpDX.RectangleF(inventoryRect.TopLeft.X + 18, inventoryRect.TopLeft.Y - 37, 37, 37);

                Graphics.DrawImage("pickL.png", pickButtonRect);
                if (isButtonPressed(pickButtonRect))
                {
                    foreach (var item in inventoryItems)
                    {
                        if (!CheckIgnoreCells(item))
                        {
                            moveItem(item.GetClientRect().Center);
                        }
                    }
                }
            }
        }

        public IList<NormalInventoryItem> GetHighlightedItems()
        {
            List<NormalInventoryItem> highlightedItems = new List<NormalInventoryItem>();
            try
            {
                IList<NormalInventoryItem> stashItems =
                    ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems;

                IOrderedEnumerable<NormalInventoryItem> orderedInventoryItems = stashItems
                    .Cast<NormalInventoryItem>()
                    .OrderBy(stashItem => stashItem.InventPosX)
                    .ThenBy(stashItem => stashItem.InventPosY);

                foreach (var item in orderedInventoryItems)
                {
                    bool isHighlighted = item.isHighlighted;
                    if (isHighlighted)
                    {
                        highlightedItems.Add(item);
                    }
                }
            }
            catch (System.Exception)
            {
            }

            return highlightedItems;
        }

        public void moveItem(SharpDX.Vector2 itemPosition)
        {
            itemPosition += windowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            Mouse.moveMouse(itemPosition);
            Mouse.LeftDown(Settings.ExtraDelay);
            Mouse.LeftUp(0);
            Keyboard.KeyUp(Keys.LControlKey);
        }

        public bool isButtonPressed(SharpDX.RectangleF buttonRect)
        {
            if (Control.MouseButtons == MouseButtons.Left)
            {
                var prevMousePos = Mouse.GetCursorPosition();

                if (buttonRect.Contains(Mouse.GetCursorPosition() - windowOffset))
                {
                    return true;
                }

                Mouse.moveMouse(prevMousePos);
            }

            return false;
        }


        private bool CheckIgnoreCells(NormalInventoryItem inventItem)
        {
            var inventPosX = inventItem.InventPosX;
            var inventPosY = inventItem.InventPosY;

            if (inventPosX < 0 || inventPosX >= 12)
                return true;
            if (inventPosY < 0 || inventPosY >= 5)
                return true;

            return Settings.IgnoredCells[inventPosY, inventPosX] != 0; //No need to check all item size
        }


        private void DrawIgnoredCellsSettings()
        {
            ImGui.BeginChild("##IgnoredCellsMain", new Vector2(ImGuiNative.igGetContentRegionAvail().X, 204f), true,
                (ImGuiWindowFlags) 16);
            ImGui.Text("Ignored Inventory Slots (checked = ignored)");

            Vector2 contentRegionAvail = ImGuiNative.igGetContentRegionAvail();
            ImGui.BeginChild("##IgnoredCellsCels", new Vector2(contentRegionAvail.X, contentRegionAvail.Y), true,
                (ImGuiWindowFlags) 16);

            int num = 1;
            for (int index1 = 0; index1 < 5; ++index1)
            {
                for (int index2 = 0; index2 < 12; ++index2)
                {
                    bool boolean = Convert.ToBoolean(Settings.IgnoredCells[index1, index2]);
                    if (ImGui.Checkbox(string.Format("##{0}IgnoredCells", (object) num), ref boolean))
                        Settings.IgnoredCells[index1, index2] ^= 1;
                    if ((num - 1) % 12 < 11)
                        ImGui.SameLine();
                    ++num;
                }
            }

            ImGui.EndChild();
            ImGui.EndChild();
        }
    }
}