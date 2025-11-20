
using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Network;
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

        public NetworkManager NetworkManager { get; private set; }

        public DeepWoodsGame Game { get; private set; }
        public bool IsLocalCoop => (Game?.PlayerManager?.LocalPlayers?.Count ?? 0) > 1;

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
                if (hostButton.IsClicked(mouse))
                {
                    saveData = new()
                    {
                        GridSize = 128,
                        NumPlayers = 3,
                        Seed = rng.Next()
                    };
                    Game = new DeepWoodsGame(this);
                    Game.StartGame(saveData);
                    Game.PlayerManager.SpawnLocalPlayer();
                    NetworkManager = NetworkManager.StartHost(new LANNetwork());
                }
                if (joinButton.IsClicked(mouse))
                {
                    Game = new DeepWoodsGame(this);
                    // TODO: get host string from UI
                    string host = "127.0.0.1";
                    NetworkManager = NetworkManager.StartClient(new LANNetwork(), host);
                }
                if (newButton.IsClicked(mouse))
                {
                    saveData = new ()
                    {
                        GridSize = 128,
                        NumPlayers = 3,
                        Seed = rng.Next()
                    };
                    Game = new DeepWoodsGame(this);
                    Game.StartGame(saveData);
                    Game.PlayerManager.SpawnLocalPlayer();
                }
                if (loadButton.IsClicked(mouse))
                {
                    saveData = SaveLoadHelper.Instance.Load("test");
                    Game = new DeepWoodsGame(this);
                    Game.StartGame(saveData);
                    Game.PlayerManager.SpawnLocalPlayer();
                }
                if (saveButton.IsClicked(mouse))
                {
                    SaveLoadHelper.Instance.Save("test", saveData);
                }
                if (Game != null)
                {
                    if (addLocalPlayerButton.IsClicked(mouse))
                    {
                        Game.PlayerManager.SpawnLocalPlayer();
                    }
                    if (addRemotePlayerButton.IsClicked(mouse))
                    {
                        Game.PlayerManager.SpawnRemotePlayer();
                    }
                }
            }

            Game?.Update(gameTime);

            NetworkManager?.Update();

            base.Update(gameTime);
        }

        private readonly Button newButton = new(new(200, 200, 400, 80), "new");
        private readonly Button loadButton = new(new(200, 320, 400, 80), "load");
        private readonly Button saveButton = new(new(200, 440, 400, 80), "save");
        private readonly Button addLocalPlayerButton = new(new(200, 560, 400, 80), "add local");
        private readonly Button addRemotePlayerButton = new(new(200, 680, 400, 80), "add remote");
        private readonly Button hostButton = new(new(200, 800, 400, 80), "host");
        private readonly Button joinButton = new(new(200, 920, 400, 80), "join");

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Game?.Draw(gameTime);

            if (Game == null || Game.isGamePaused)
            {
                spriteBatch.Begin();
                newButton.Draw(spriteBatch);
                loadButton.Draw(spriteBatch);
                saveButton.Draw(spriteBatch);
                addLocalPlayerButton.Draw(spriteBatch);
                addRemotePlayerButton.Draw(spriteBatch);
                hostButton.Draw(spriteBatch);
                joinButton.Draw(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
