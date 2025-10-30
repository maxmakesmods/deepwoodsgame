
using DeepWoods.Game;
using DeepWoods.Graphics;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static DeepWoods.Helpers.SaveLoadHelper;

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
        private SaveLoadHelper.SaveData saveData;

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
                var mouse = Mouse.GetState();
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    if (newButtonRectangle.Contains(mouse.X, mouse.Y) && !newbuttonpressed)
                    {
                        saveData = new ()
                        {
                            GridSize = 128,
                            NumPlayers = 3,
                            Seed = rng.Next()
                        };
                        game = new DeepWoodsGame(this, saveData);
                        newbuttonpressed = true;
                    }
                    if (loadButtonRectangle.Contains(mouse.X, mouse.Y) && !loadbuttonpressed)
                    {
                        saveData = SaveLoadHelper.Instance.Load("test");
                        game = new DeepWoodsGame(this, saveData);
                        loadbuttonpressed = true;
                    }
                    if (saveButtonRectangle.Contains(mouse.X, mouse.Y) && !savebuttonpressed)
                    {
                        SaveLoadHelper.Instance.Save("test", saveData);
                        savebuttonpressed = true;
                    }
                    if (game != null)
                    {
                        if (addLocalPlayerButtonRectangle.Contains(mouse.X, mouse.Y) && !addLocalPlayerbuttonpressed)
                        {
                            game.PlayerManager.SpawnLocalPlayer();
                            addLocalPlayerbuttonpressed = true;
                        }
                        if (addRemotePlayerButtonRectangle.Contains(mouse.X, mouse.Y) && !addRemotePlayerbuttonpressed)
                        {
                            game.PlayerManager.SpawnRemotePlayer();
                            addRemotePlayerbuttonpressed = true;
                        }
                    }
                }
                else
                {
                    newbuttonpressed = false;
                    loadbuttonpressed = false;
                    savebuttonpressed = false;
                    addLocalPlayerbuttonpressed = false;
                    addRemotePlayerbuttonpressed = false;
                }
            }

            if (game != null)
            {
                game.Update(gameTime);
            }

            base.Update(gameTime);
        }

        Rectangle newButtonRectangle = new Rectangle(200, 200, 400, 80);
        Rectangle loadButtonRectangle = new Rectangle(200, 320, 400, 80);
        Rectangle saveButtonRectangle = new Rectangle(200, 440, 400, 80);
        Rectangle addLocalPlayerButtonRectangle = new Rectangle(200, 560, 400, 80);
        Rectangle addRemotePlayerButtonRectangle = new Rectangle(200, 680, 400, 80);
        private bool newbuttonpressed = false;
        private bool loadbuttonpressed = false;
        private bool savebuttonpressed = false;
        private bool addLocalPlayerbuttonpressed = false;
        private bool addRemotePlayerbuttonpressed = false;

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
                spriteBatch.Draw(TextureLoader.WhiteTexture, newButtonRectangle, new Color(1, 1, 1, 0.5f));
                spriteBatch.Draw(TextureLoader.WhiteTexture, loadButtonRectangle, new Color(1, 1, 1, 0.5f));
                spriteBatch.Draw(TextureLoader.WhiteTexture, saveButtonRectangle, new Color(1, 1, 1, 0.5f));
                spriteBatch.Draw(TextureLoader.WhiteTexture, addLocalPlayerButtonRectangle, new Color(1, 1, 1, 0.5f));
                spriteBatch.Draw(TextureLoader.WhiteTexture, addRemotePlayerButtonRectangle, new Color(1, 1, 1, 0.5f));
                TextHelper.DrawStringOnScreen(spriteBatch, newButtonRectangle.Position(), "new", Color.Red);
                TextHelper.DrawStringOnScreen(spriteBatch, loadButtonRectangle.Position(), "load", Color.Red);
                TextHelper.DrawStringOnScreen(spriteBatch, saveButtonRectangle.Position(), "save", Color.Red);
                TextHelper.DrawStringOnScreen(spriteBatch, addLocalPlayerButtonRectangle.Position(), "add local", Color.Red);
                TextHelper.DrawStringOnScreen(spriteBatch, addRemotePlayerButtonRectangle.Position(), "add remote", Color.Red);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
