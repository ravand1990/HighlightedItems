// Decompiled with JetBrains decompiler
// Type: HighlightedItems.HighlightedItems
// Assembly: HighlightedItems, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 74CED332-D7C7-49BB-A79B-828BCE83C865
// Assembly location: C:\Users\admin\Desktop\HighlightedItems.dll

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
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HighlightedItems
{
  public class HighlightedItems : BaseSettingsPlugin<Settings>
  {
    private IngameState ingameState;
    private SharpDX.Vector2 windowOffset;

    public override bool Initialise()
    {
      base.Initialise();
      this.Name = nameof (HighlightedItems);
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
      if (!(bool) this.Settings.Enable)
        return;
      StashElement stashElement = this.ingameState.IngameUi.StashElement;
      if (stashElement.IsVisible)
      {
        Inventory visibleStash = stashElement.VisibleStash;
        if (visibleStash == null)
          return;
        RectangleF clientRect = visibleStash.InventoryUIElement.GetClientRect();
        RectangleF rectangleF = new RectangleF(clientRect.BottomRight.X - 43f, clientRect.BottomRight.Y + 10f, 37f, 37f);
        this.Graphics.DrawImage("pick.png", rectangleF);
        IList<NormalInventoryItem> highlightedItems = this.GetHighlightedItems();
        int? nullable1 = new int?(0);
        foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>) highlightedItems)
        {
          int? nullable2 = nullable1;
          int? size = normalInventoryItem.Item?.GetComponent<Stack>()?.Size;
          nullable1 = nullable2.HasValue & size.HasValue ? new int?(nullable2.GetValueOrDefault() + size.GetValueOrDefault()) : new int?();
        }
        string str;
        if ((bool) this.Settings.ShowStackSizes)
        {
          int count = highlightedItems.Count;
          int? nullable3 = nullable1;
          int valueOrDefault = nullable3.GetValueOrDefault();
          if (!(count == valueOrDefault & nullable3.HasValue) && nullable1.HasValue)
          {
            str = !(bool) this.Settings.ShowStackCountWithSize ? string.Format("{0}", (object) nullable1) : string.Format("{0} / {1}", (object) nullable1, (object) highlightedItems.Count);
            goto label_17;
          }
        }
        str = string.Format("{0}", (object) highlightedItems.Count);
label_17:
        System.Numerics.Vector2 vector2 = new System.Numerics.Vector2(rectangleF.Left - 2f, rectangleF.Center.Y);
        vector2.Y -= 11f;
        this.Graphics.DrawText(str ?? "", new System.Numerics.Vector2(vector2.X, vector2.Y + 2f), Color.Black, 10, "FrizQuadrataITC:22", FontAlign.Right);
        this.Graphics.DrawText(str ?? "", new System.Numerics.Vector2(vector2.X - 2f, vector2.Y), Color.White, 10, "FrizQuadrataITC:22", FontAlign.Right);
        if (this.isButtonPressed(rectangleF) || Keyboard.IsKeyPressed(this.Settings.HotKey.Value))
        {
          Point cursorPosition = Mouse.GetCursorPosition();
          foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>) highlightedItems)
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
            this.moveItem(normalInventoryItem.GetClientRect().Center);
          }
          Mouse.moveMouse((SharpDX.Vector2) cursorPosition);
        }
      }
      InventoryElement inventoryPanel = this.ingameState.IngameUi.InventoryPanel;
      IList<NormalInventoryItem> visibleInventoryItems = inventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
      if (!inventoryPanel.IsVisible || !(bool) this.Settings.DumpButtonEnable)
        return;
      RectangleF clientRect1 = inventoryPanel.Children[2].GetClientRect();
      RectangleF rectangleF1 = new RectangleF(clientRect1.TopLeft.X + 18f, clientRect1.TopLeft.Y - 37f, 37f, 37f);
      this.Graphics.DrawImage("pickL.png", rectangleF1);
      if (!this.isButtonPressed(rectangleF1))
        return;
      foreach (NormalInventoryItem inventItem in (IEnumerable<NormalInventoryItem>) visibleInventoryItems)
      {
        if (!this.CheckIgnoreCells(inventItem))
        {
          if (!this.ingameState.IngameUi.InventoryPanel.IsVisible)
          {
            DebugWindow.LogMsg("HighlightedItems: Inventory Panel closed, aborting loop");
            break;
          }
          if (!this.ingameState.IngameUi.StashElement.IsVisible && !this.ingameState.IngameUi.OpenLeftPanel.IsVisible && !this.ingameState.IngameUi.TradeWindow.IsVisible && !this.ingameState.IngameUi.SellWindow.IsVisible && !this.ingameState.IngameUi.SellWindowHideout.IsVisible)
          {
            DebugWindow.LogMsg("HighlightedItems: No window open to dump to.");
            break;
          }

          this.moveItem(inventItem.GetClientRect().Center);
        }
      }
    }

    public IList<NormalInventoryItem> GetHighlightedItems()
    {
      List<NormalInventoryItem> highlightedItems = new List<NormalInventoryItem>();
      try
      {
        foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>) this.ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems.Cast<NormalInventoryItem>().OrderBy<NormalInventoryItem, int>((Func<NormalInventoryItem, int>) (stashItem => stashItem.InventPosX)).ThenBy<NormalInventoryItem, int>((Func<NormalInventoryItem, int>) (stashItem => stashItem.InventPosY)))
        {
          if (normalInventoryItem.isHighlighted)
            highlightedItems.Add(normalInventoryItem);
        }
      }
      catch (Exception ex)
      {
      }
      return (IList<NormalInventoryItem>) highlightedItems;
    }

    private bool IsInventoryFull()
    {
      IList<NormalInventoryItem> visibleInventoryItems = this.ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
      if (visibleInventoryItems.Count < 12)
        return false;
      bool[,] flagArray = new bool[12, 5];
      foreach (NormalInventoryItem normalInventoryItem in (IEnumerable<NormalInventoryItem>) visibleInventoryItems)
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

    public void moveItem(SharpDX.Vector2 itemPosition)
    {
      itemPosition += this.windowOffset;
      Keyboard.KeyDown(Keys.LControlKey);
      Mouse.moveMouse(itemPosition);
      Mouse.LeftDown((int) this.Settings.ExtraDelay);
      Mouse.LeftUp(0);
      Keyboard.KeyUp(Keys.LControlKey);
    }

    public bool isButtonPressed(RectangleF buttonRect)
    {
      if (Control.MouseButtons == MouseButtons.Left)
      {
        Point cursorPosition = Mouse.GetCursorPosition();
        if (buttonRect.Contains((SharpDX.Vector2) Mouse.GetCursorPosition() - this.windowOffset))
          return true;
        Mouse.moveMouse((SharpDX.Vector2) cursorPosition);
      }
      return false;
    }

    private bool CheckIgnoreCells(NormalInventoryItem inventItem)
    {
      int inventPosX = inventItem.InventPosX;
      int inventPosY = inventItem.InventPosY;
      return inventPosX < 0 || inventPosX >= 12 || inventPosY < 0 || inventPosY >= 5 || (uint) this.Settings.IgnoredCells[inventPosY, inventPosX] > 0U;
    }

    private void DrawIgnoredCellsSettings()
    {
      ImGui.BeginChild("##IgnoredCellsMain", new System.Numerics.Vector2(ImGuiNative.igGetContentRegionAvail().X, 204f), true, ImGuiWindowFlags.NoScrollWithMouse);
      ImGui.Text("Ignored Inventory Slots (checked = ignored)");
      System.Numerics.Vector2 contentRegionAvail = ImGuiNative.igGetContentRegionAvail();
      ImGui.BeginChild("##IgnoredCellsCels", new System.Numerics.Vector2(contentRegionAvail.X, contentRegionAvail.Y), true, ImGuiWindowFlags.NoScrollWithMouse);
      int num = 1;
      for (int index1 = 0; index1 < 5; ++index1)
      {
        for (int index2 = 0; index2 < 12; ++index2)
        {
          bool boolean = Convert.ToBoolean(this.Settings.IgnoredCells[index1, index2]);
          if (ImGui.Checkbox(string.Format("##{0}IgnoredCells", (object) num), ref boolean))
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
