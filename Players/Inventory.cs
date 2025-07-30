using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeepWoods.Players
{
    internal class Inventory
    {
        private static readonly int NumSlots = 10;

        private class InventorySlot
        {
            public DWObject dwobj;
            public int count;
        }

        private readonly List<InventorySlot> slots = [];

        public Inventory(GraphicsDevice graphicsDevice)
        {
        }

        public void DrawUI(GraphicsDevice graphicsDevice, TextHelper textHelper, SpriteBatch spriteBatch)
        {
            int w = graphicsDevice.Viewport.Width;
            int h = graphicsDevice.Viewport.Height;

            int barheight = (int)(h * 0.1);
            int barwidth = barheight * NumSlots;

            int barx = (w - barwidth) / 2;
            int bary = (int)(h - barheight * 1.5);

            spriteBatch.Draw(TextureLoader.WhiteTexture, new Rectangle(barx, bary, barwidth, barheight), Color.BurlyWood);

            for (int i = 0; i < slots.Count && i < NumSlots; i++)
            {
                if (slots[i].dwobj != null)
                {
                    spriteBatch.Draw(TextureLoader.ObjectsTexture, new Rectangle(barx + barheight * i, bary, barheight, barheight), slots[i].dwobj.TexRect, Color.White);
                    textHelper.DrawStringOnScreen(spriteBatch, new(barx + barheight * i, bary + barheight), slots[i].count.ToString());
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
