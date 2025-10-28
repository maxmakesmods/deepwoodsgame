using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Main;
using DeepWoods.Objects;
using DeepWoods.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeepWoods.Players
{
    internal class Inventory : UIPanel
    {
        private static readonly int NumRowSlots = 10;
        private static readonly int NumRows = 7;

        public bool IsOpen { get; set; } = false;

        private class InventorySlot
        {
            public DWObject dwobj;
            public int count;
        }

        private readonly List<InventorySlot> slots = [];

        public Inventory()
        {
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            PanelData panelData = DoThePanelThing(spriteBatch, NumRowSlots, 1, 0.85f);

            for (int i = 0; i < slots.Count && i < NumRowSlots; i++)
            {
                if (slots[i].dwobj != null)
                {
                    var cell = panelData.MakeCell(i, 0);
                    spriteBatch.Draw(TextureLoader.ObjectsTexture, cell, slots[i].dwobj.TexRect, Color.White);
                    DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, cell.Position(), slots[i].count.ToString());
                }
            }

            if (IsOpen)
            {
                DrawOpenInventory(spriteBatch);
            }
        }

        private void DrawOpenInventory(SpriteBatch spriteBatch)
        {
            var panelData = DoThePanelThing(spriteBatch, NumRowSlots, NumRows, 0.1f);

            for (int i = NumRowSlots; i < slots.Count; i++)
            {
                if (slots[i].dwobj != null)
                {
                    int slotIndex = i - NumRowSlots;
                    var cell = panelData.MakeCell(slotIndex);
                    spriteBatch.Draw(TextureLoader.ObjectsTexture, cell, slots[i].dwobj.TexRect, Color.White);
                    DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, cell.Position(), slots[i].count.ToString());
                }
            }
        }

        public void Add(DWObject dwobj)
        {
            foreach (var slot in slots)
            {
                if (slot.dwobj.Def == dwobj.Def)
                {
                    slot.count++;
                    return;
                }
            }
            slots.Add(new()
            {
                dwobj = dwobj,
                count = 1
            });
        }
    }
}
