using DeepWoods.Loaders;
using DeepWoods.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeepWoods.Players
{
    internal class Inventory
    {
        public readonly List<Sprite> objects = [];
        private readonly Texture2D whiteTexture;

        public Inventory(GraphicsDevice graphicsDevice)
        {
            whiteTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            whiteTexture.SetData(0, null, [Color.White], 0, 1);
        }

        public void DrawUI(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            int w = graphicsDevice.Viewport.Width;
            int h = graphicsDevice.Viewport.Height;

            int barx = (int)(w * 0.1);
            int bary = (int)(h * 0.7);
            int barwidth = (int)(w * 0.8);
            int barheight = (int)(h * 0.2);

            spriteBatch.Draw(whiteTexture, new Rectangle(barx, bary, barwidth, barheight), Color.BurlyWood);

            for (int i = 0; i < objects.Count; i++)
            {
                spriteBatch.Draw(TextureLoader.ObjectsTexture, new Rectangle(barx + barheight * i, bary, barheight, barheight), objects[i].TexRect, Color.White);
            }

        }
    }
}
