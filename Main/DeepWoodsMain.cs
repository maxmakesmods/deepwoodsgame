
using DeepWoods.Game;
using DeepWoods.Graphics;
using DeepWoods.Loaders;
using DeepWoods.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods.Main
{
    public class DeepWoodsMain : Microsoft.Xna.Framework.Game
    {
        public static DeepWoodsMain Instance { get; private set; }

        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public FPSCounter FPS { get; private set; }
        public TextHelper TextHelper { get; private set; }

        private DeepWoodsGame game;

        private SpriteBatch spriteBatch;

        private readonly System.Random rng;


        private bool buttonpressed = false;


        public DeepWoodsMain()
        {
            Instance = this;

            rng = new System.Random();

            Content.RootDirectory = "Content";
            GraphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false,
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };
            GraphicsDeviceManager.ApplyChanges();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            IsFixedTimeStep = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            FPS = new FPSCounter();
            TextHelper = new TextHelper();

            EffectLoader.Load(Content);
            TextureLoader.Load(Content, GraphicsDevice);

            RawInput.Initialize(WindowHelper.GetRealHWNDFromSDL(Window.Handle));

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            FPS.CountFrame(gameTime);

            if (IsActive)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (!buttonpressed)
                    {
                        game = new DeepWoodsGame(this, rng.Next(), 1, 128);
                    }
                    buttonpressed = true;
                }
                else
                {
                    buttonpressed = false;
                }
            }

            if (game != null)
            {
                game.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (game != null)
            {
                game.Draw(gameTime);
            }

            if (game == null || game.isGamePaused)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(TextureLoader.WhiteTexture, new Rectangle(200,200,400,80), new Color(1, 1, 1, 0.5f));
                //TextHelper.
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
