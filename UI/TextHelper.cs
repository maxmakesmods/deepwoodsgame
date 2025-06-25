
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.UI
{
    internal class TextHelper
    {
        private SpriteFont ft88RegularFont;

        public TextHelper(GraphicsDevice graphicsDevice, ContentManager content)
        {
            ft88RegularFont = content.Load<SpriteFont>("fonts/FT88-Regular");
        }

        public void DrawStringOnScreen(SpriteBatch spriteBatch, string text)
        {
            spriteBatch.DrawString(ft88RegularFont, text, new Vector2(22f, 22f), Color.Black);
            spriteBatch.DrawString(ft88RegularFont, text, new Vector2(20f, 20f), Color.White);
        }
    }
}
