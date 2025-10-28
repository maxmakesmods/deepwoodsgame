
using DeepWoods.Game;
using DeepWoods.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.UI
{
    public class TextHelper
    {
        private SpriteFont ft88RegularFont;

        public TextHelper()
        {
            ft88RegularFont = DeepWoodsMain.Instance.Content.Load<SpriteFont>("fonts/FT88-Regular");
        }

        public void DrawStringOnScreen(SpriteBatch spriteBatch, Vector2 position, string text)
        {
            DrawStringOnScreen(spriteBatch, position, text, Color.White);
        }

        public void DrawStringOnScreen(SpriteBatch spriteBatch, Vector2 position, string text, Color color)
        {
            spriteBatch.DrawString(ft88RegularFont, text, position + new Vector2(2f, 2f), Color.Black);
            spriteBatch.DrawString(ft88RegularFont, text, position, color);
        }
    }
}
