
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods.UI
{
    internal class Button
    {
        private readonly Rectangle rectangle;
        private readonly string label;

        private bool pressed = false;

        public Button(Rectangle rectangle, string label)
        {
            this.rectangle = rectangle;
            this.label = label;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureLoader.WhiteTexture, rectangle, new Color(1, 1, 1, 0.5f));
            DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, rectangle.Position(), label, Color.Red);
        }

        public bool IsClicked(MouseState mouse)
        {
            if (mouse.LeftButton != ButtonState.Pressed)
            {
                pressed = false;
            }
            else if (rectangle.Contains(mouse.X, mouse.Y) && !pressed)
            {
                pressed = true;
                return true;
            }
            return false;
        }
    }
}
