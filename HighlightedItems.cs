using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using HighlightedItems.Utils;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using RectangleF = SharpDX.RectangleF;
using Color = SharpDX.Color;
using Point = SharpDX.Point;

namespace HighlightedItems
{
    public class HighlightedItems : BaseSettingsPlugin<Settings>
    {
        private IngameState ingameState;
        private SharpDX.Vector2 windowOffset;
        private IEnumerator<bool> _currentOperation;
        private static readonly TimeSpan KeyDelay = TimeSpan.FromMilliseconds(10);
        private static readonly TimeSpan MouseMoveDelay = TimeSpan.FromMilliseconds(1);
        private static readonly TimeSpan MouseUpDelay = TimeSpan.FromMilliseconds(1);

        private bool MoveCancellationRequested => (bool)this.Settings.CancelWithRightMouseButton && (uint)(Control.MouseButtons & MouseButtons.Right) > 0U;

        public override bool Initialise()
        {
            base.Initialise();
            this.Name = nameof(HighlightedItems);
            this.ingameState = this.GameController.Game.IngameState;
            this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;
            string name1 = Path.Combine(this.DirectoryFullName, "images\\pick.png").Replace('\\', '/');
            string name2 = Path.Combine(this.DirectoryFullName, "images\\pickL.png").Replace('\\', '/');
            this.Graphics.InitImage(name1, false);
            this.Graphics.InitImage(name2, false);
            return true;
        }

        public override void DrawSettings()
        {
            base.DrawSettings();
            this.DrawIgnoredCellsSettings();
        }

        public override void Render()
        {
            if (this._currentOperation != null && this._currentOperation.MoveNext())
            {
                DebugWindow.LogMsg("Running the inventory dump procedure...");
            }
            else
            {
                if (!(bool)this.Settings.Enable)
                    return;

                StashElement stashElement = this.ingameState.IngameUi.StashElement;
                if (stashElement.IsVisible)
                {
                    this.Graphics.DrawFrame(stashElement.GetClientRect(), Color.Red, 2);
                    Inventory visibleStash = stashElement.VisibleStash;
                    if (visibleStash != null)
                    {
                        RectangleF clientRect = visibleStash.InventoryUIElement.GetClientRect();
                        RectangleF rectangleF = new RectangleF(clientRect.BottomRight.X - 43f, clientRect.BottomRight.Y + 10f, 37f, 37f);
                        this.Graphics.DrawImage("pick.png", rectangleF);
                        IList<NormalInventoryItem> highlightedItems = this.GetHighlightedItems();
                        int? nullable1 = new int?(0);
                        foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>)highlightedItems)
                        {
                            int? nullable2 = nullable1;
                            int? size = normalInventoryItem.Item?.GetComponent<Stack>()?.Size;
                            nullable1 = nullable2.HasValue & size.HasValue ? new int?(nullable2.GetValueOrDefault() + size.GetValueOrDefault()) : new int?();
                        }
                        string stringAndClear;
                        if ((bool)this.Settings.ShowStackSizes)
                        {
                            int count = highlightedItems.Count;
                            int? nullable3 = nullable1;
                            int valueOrDefault = nullable3.GetValueOrDefault();
                            if (!(count == valueOrDefault & nullable3.HasValue) && nullable1.HasValue)
                            {
                                if ((bool)this.Settings.ShowStackCountWithSize)
                                {
                                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
                                    interpolatedStringHandler.AppendFormatted<int?>(nullable1);
                                    interpolatedStringHandler.AppendLiteral(" / ");
                                    interpolatedStringHandler.AppendFormatted<int>(highlightedItems.Count);
                                    stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                }
                                else
                                {
                                    DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
                                    interpolatedStringHandler.AppendFormatted<int?>(nullable1);
                                    stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                }
                            }

                        }
                        DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(0, 1);
                        interpolatedStringHandler1.AppendFormatted<int>(highlightedItems.Count);
                        stringAndClear = interpolatedStringHandler1.ToStringAndClear();

                        System.Numerics.Vector2 vector2 = new System.Numerics.Vector2(rectangleF.Left - 2f, rectangleF.Center.Y);
                        vector2.Y -= 11f;
                        this.Graphics.DrawText(stringAndClear ?? "", new System.Numerics.Vector2(vector2.X, vector2.Y + 2f), Color.Black, 10, "FrizQuadrataITC:22", FontAlign.Right);
                        this.Graphics.DrawText(stringAndClear ?? "", new System.Numerics.Vector2(vector2.X - 2f, vector2.Y), Color.White, 10, "FrizQuadrataITC:22", FontAlign.Right);
                        if (this.isButtonPressed(rectangleF) || Keyboard.IsKeyPressed(this.Settings.HotKey.Value))
                            this._currentOperation = this.MoveItemsToInventory((IList<NormalInventoryItem>)highlightedItems.OrderBy<NormalInventoryItem, int>((Func<NormalInventoryItem, int>)(stashItem => stashItem.InventPosX)).ThenBy<NormalInventoryItem, int>((Func<NormalInventoryItem, int>)(stashItem => stashItem.InventPosY)).ThenBy<NormalInventoryItem, float>((Func<NormalInventoryItem, float>)(stashItem => stashItem.GetClientRectCache.X)).ThenBy<NormalInventoryItem, float>((Func<NormalInventoryItem, float>)(stashItem => stashItem.GetClientRectCache.Y)).ToList<NormalInventoryItem>()).GetEnumerator();
                    }

                }

                InventoryElement inventoryPanel = this.ingameState.IngameUi.InventoryPanel;
                if (!inventoryPanel.IsVisible || !(bool)this.Settings.DumpButtonEnable)
                    return;
                RectangleF clientRect1 = inventoryPanel.Children[2].GetClientRect();
                RectangleF rectangleF1 = new RectangleF(clientRect1.TopLeft.X + 18f, clientRect1.TopLeft.Y - 37f, 37f, 37f);
                this.Graphics.DrawImage("pickL.png", rectangleF1);
                if (!this.isButtonPressed(rectangleF1))
                    return;
                this._currentOperation = this.MoveItemsToStash((IList<NormalInventoryItem>)inventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems.OrderBy<NormalInventoryItem, int>((Func<NormalInventoryItem, int>)(x => x.InventPosX)).ThenBy<NormalInventoryItem, int>((Func<NormalInventoryItem, int>)(x => x.InventPosY)).ToList<NormalInventoryItem>()).GetEnumerator();
            }
        }

        private IEnumerable<bool> MoveItemsCommonPreamble(CancellationTokenSource cts)
        {
            HighlightedItems highlightedItems = this;
            while (Control.MouseButtons == MouseButtons.Left)
            {
                if (highlightedItems.MoveCancellationRequested)
                {
                    cts.Cancel();
                    yield break;
                }
                else
                    yield return false;
            }
            if (highlightedItems.Settings.IdleMouseDelay.Value != 0)
            {
                Point mousePos = Mouse.GetCursorPosition();
                Stopwatch sw = Stopwatch.StartNew();
                yield return false;
                while (!highlightedItems.MoveCancellationRequested)
                {
                    Point cursorPosition = Mouse.GetCursorPosition();
                    if (mousePos != cursorPosition)
                    {
                        mousePos = cursorPosition;
                        sw.Restart();
                    }
                    else if (sw.ElapsedMilliseconds >= (long)highlightedItems.Settings.IdleMouseDelay.Value)
                        yield break;
                    else
                        yield return false;
                }
                cts.Cancel();
            }
        }

        private IEnumerable<bool> MoveItemsToStash(IList<NormalInventoryItem> items)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            foreach (bool flag in this.MoveItemsCommonPreamble(cts))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
            if (!cts.Token.IsCancellationRequested)
            {
                Point prevMousePos = Mouse.GetCursorPosition();
                foreach (NormalInventoryItem inventItem in (IEnumerable<NormalInventoryItem>)items)
                {
                    if (this.MoveCancellationRequested)
                        yield break;
                    else if (!this.CheckIgnoreCells(inventItem))
                    {
                        if (!this.ingameState.IngameUi.InventoryPanel.IsVisible)
                        {
                            DebugWindow.LogMsg("HighlightedItems: Inventory Panel closed, aborting loop");
                            break;
                        }
                        foreach (bool flag in this.MoveItem(inventItem.GetClientRect().Center))
                        {
                            int num = flag ? 1 : 0;
                            yield return false;
                        }
                    }
                }
                Mouse.moveMouse((SharpDX.Vector2)prevMousePos);
                foreach (bool flag in this.Wait(HighlightedItems.MouseMoveDelay, true))
                {
                    int num = flag ? 1 : 0;
                    yield return false;
                }
            }
        }

        private IEnumerable<bool> MoveItemsToInventory(IList<NormalInventoryItem> items)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            foreach (bool flag in this.MoveItemsCommonPreamble(cts))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
            if (!cts.Token.IsCancellationRequested)
            {
                Point prevMousePos = Mouse.GetCursorPosition();
                foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>)items)
                {
                    if (this.MoveCancellationRequested)
                    {
                        yield break;
                    }
                    else
                    {
                        if (!this.ingameState.IngameUi.StashElement.IsVisible)
                        {
                            DebugWindow.LogMsg("HighlightedItems: Stash Panel closed, aborting loop");
                            break;
                        }
                        if (!this.ingameState.IngameUi.InventoryPanel.IsVisible)
                        {
                            DebugWindow.LogMsg("HighlightedItems: Inventory Panel closed, aborting loop");
                            break;
                        }
                        if (this.IsInventoryFull())
                        {
                            DebugWindow.LogMsg("HighlightedItems: Inventory full, aborting loop");
                            break;
                        }
                        foreach (bool flag in this.MoveItem(normalInventoryItem.GetClientRect().Center))
                        {
                            int num = flag ? 1 : 0;
                            yield return false;
                        }
                    }
                }
                Mouse.moveMouse((SharpDX.Vector2)prevMousePos);
                foreach (bool flag in this.Wait(HighlightedItems.MouseMoveDelay, true))
                {
                    int num = flag ? 1 : 0;
                    yield return false;
                }
            }
        }

        public IList<NormalInventoryItem> GetHighlightedItems()
        {
            try
            {
                return (IList<NormalInventoryItem>)this.ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems.Where<NormalInventoryItem>((Func<NormalInventoryItem, bool>)(stashItem => stashItem.isHighlighted)).ToList<NormalInventoryItem>().ToList<NormalInventoryItem>();
            }
            catch
            {
                return (IList<NormalInventoryItem>)new List<NormalInventoryItem>();
            }
        }

        private bool IsInventoryFull()
        {
            IList<NormalInventoryItem> visibleInventoryItems = this.ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            if (visibleInventoryItems.Count < 12)
                return false;
            bool[,] flagArray = new bool[12, 5];
            foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>)visibleInventoryItems)
            {
                int inventPosX = normalInventoryItem.InventPosX;
                int inventPosY = normalInventoryItem.InventPosY;
                int itemHeight = normalInventoryItem.ItemHeight;
                int itemWidth = normalInventoryItem.ItemWidth;
                for (int index1 = inventPosX; index1 < inventPosX + itemWidth; ++index1)
                {
                    for (int index2 = inventPosY; index2 < inventPosY + itemHeight; ++index2)
                        flagArray[index1, index2] = true;
                }
            }
            for (int index3 = 0; index3 < 12; ++index3)
            {
                for (int index4 = 0; index4 < 5; ++index4)
                {
                    if (!flagArray[index3, index4])
                        return false;
                }
            }
            return true;
        }

        private TimeSpan MouseDownDelay => TimeSpan.FromMilliseconds((double)(5 + this.Settings.ExtraDelay.Value));

        private IEnumerable<bool> MoveItem(SharpDX.Vector2 itemPosition)
        {
            itemPosition += this.windowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            foreach (bool flag in this.Wait(HighlightedItems.KeyDelay, true))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
            Mouse.moveMouse(itemPosition);
            foreach (bool flag in this.Wait(HighlightedItems.MouseMoveDelay, true))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
            Mouse.LeftDown(0);
            foreach (bool flag in this.Wait(this.MouseDownDelay, true))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
            Mouse.LeftUp(0);
            foreach (bool flag in this.Wait(HighlightedItems.MouseUpDelay, true))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
            Keyboard.KeyUp(Keys.LControlKey);
            foreach (bool flag in this.Wait(HighlightedItems.KeyDelay, false))
            {
                int num = flag ? 1 : 0;
                yield return false;
            }
        }

        private IEnumerable<bool> Wait(TimeSpan period, bool canUseThreadSleep)
        {
            HighlightedItems highlightedItems = this;
            if (canUseThreadSleep && (bool)highlightedItems.Settings.UseThreadSleep)
            {
                Thread.Sleep(period);
            }
            else
            {
                Stopwatch sw = Stopwatch.StartNew();
                while (sw.Elapsed < period)
                    yield return false;
            }
        }

        public bool isButtonPressed(SharpDX.RectangleF buttonRect) => Control.MouseButtons == MouseButtons.Left && buttonRect.Contains((SharpDX.Vector2)Mouse.GetCursorPosition() - this.windowOffset);

        private bool CheckIgnoreCells(NormalInventoryItem inventItem)
        {
            int inventPosX = inventItem.InventPosX;
            int inventPosY = inventItem.InventPosY;
            return inventPosX < 0 || inventPosX >= 12 || inventPosY < 0 || inventPosY >= 5 || (uint)this.Settings.IgnoredCells[inventPosY, inventPosX] > 0U;
        }

        private void DrawIgnoredCellsSettings()
        {
            ImGui.BeginChild("##IgnoredCellsMain", new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 204f), true, ImGuiWindowFlags.NoScrollWithMouse);
            ImGui.Text("Ignored Inventory Slots (checked = ignored)");
            System.Numerics.Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
            ImGui.BeginChild("##IgnoredCellsCels", new System.Numerics.Vector2(contentRegionAvail.X, contentRegionAvail.Y), true, ImGuiWindowFlags.NoScrollWithMouse);
            int num = 1;
            for (int index1 = 0; index1 < 5; ++index1)
            {
                for (int index2 = 0; index2 < 12; ++index2)
                {
                    bool boolean = Convert.ToBoolean(this.Settings.IgnoredCells[index1, index2]);
                    if (ImGui.Checkbox(string.Format("##{0}IgnoredCells", (object)num), ref boolean))
                        this.Settings.IgnoredCells[index1, index2] ^= 1;
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