using DeepWoods.Loaders;
using DeepWoods.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeepWoods.UI
{
    public class UIPanel
    {
        protected class PanelData
        {
            public int x, y;
            public int width, height;
            public int cellsize;
            public int numColumns, numRows;

            public Rectangle MakeCell(int column, int row)
            {
                return new Rectangle(x + cellsize * column, y + cellsize * row, cellsize, cellsize);
            }

            internal Rectangle MakeCell(int slotIndex)
            {
                int row = slotIndex / numColumns;
                int column = slotIndex % numColumns;
                return MakeCell(column, row);
            }

            internal Vector2 Position()
            {
                return new Vector2(x, y);
            }
        }

        protected PanelData DoThePanelThing(SpriteBatch spriteBatch, int numColumns, int numRows, float top)
        {
            int w = DeepWoodsMain.Instance.GraphicsDevice.Viewport.Width;
            int h = DeepWoodsMain.Instance.GraphicsDevice.Viewport.Height;

            int cellsize = (int)(h * 0.1);
            int width = cellsize * numColumns;
            int height = cellsize * numRows;

            int x = (w - width) / 2;
            int y = (int)(h * top);

            spriteBatch.Draw(TextureLoader.WhiteTexture, new Rectangle(x, y, width, height), Color.BurlyWood);

            return new PanelData()
            {
                x = x,
                y = y,
                cellsize = cellsize,
                width = width,
                height = height,
                numColumns = numColumns,
                numRows = numRows
            };
        }
    }
}
