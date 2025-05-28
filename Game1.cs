using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D ballTexture;
        Vector2 ballPos;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ballTexture = Content.Load<Texture2D>("ball");
            ballPos = Vector2.Zero;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            Vector2 mousePos = new(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);

            Rectangle rect = new((int)ballPos.X, (int)ballPos.Y, 100, 100);

            if (rect.Contains(mousePos) || Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                var rand = new Random();
                int x = rand.Next(0, Window.ClientBounds.Right - Window.ClientBounds.Left - 100);
                int y = rand.Next(0, Window.ClientBounds.Bottom - Window.ClientBounds.Top - 100);
                ballPos = new Vector2(x, y);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(ballTexture, ballPos, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
