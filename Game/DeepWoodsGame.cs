using DeepWoods.Graphics;
using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DeepWoods.Game
{
    public class DeepWoodsGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;

        private int gridSize = 32;
        private int numPatches = 10;

        private Terrain terrain;
        private LightManager lightManager;
        private ObjectManager objectManager;
        private InGameClock clock;
        private Random rng = new Random();
        private DWRenderer renderer;
        private PlayerManager playerManager;

        public static GameWindow window;

        private FPSCounter fps = new();

        public DeepWoodsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            window = Window;
        }

        protected override void Initialize()
        {
            base.Initialize();

            RawInput.Initialize(WindowHelper.GetRealHWNDFromSDL(Window.Handle));

            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GameState.IsMultiplayerGame = true;
        }

        protected override void LoadContent()
        {
            EffectLoader.Load(Content);
            TextureLoader.Load(Content, GraphicsDevice);

            playerManager = new PlayerManager(rng.Next());
            renderer = new DWRenderer(GraphicsDevice, Content);

            clock = new InGameClock();
            clock.TimeScale = 0;
            clock.SetTime(1, 12, 0);


            terrain = new Terrain(GraphicsDevice, rng.Next(), gridSize, gridSize, numPatches);
            terrain.Apply();

            lightManager = new LightManager(rng.Next(), gridSize, gridSize);
            objectManager = new ObjectManager(Content, GraphicsDevice, rng.Next(), gridSize, gridSize, terrain);

            int numPlayers = 1;
            playerManager.SpawnPlayers(GraphicsDevice, terrain, numPlayers);
        }

        private bool wasESCPressed = false;
        private bool isGamePaused = false;

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (!wasESCPressed)
                {
                    isGamePaused = !isGamePaused;
                    if (isGamePaused)
                    {
                        MouseState ms = DWMouse.GetState(playerManager.Players[0]);
                        Mouse.SetPosition(ms.X, ms.Y);
                    }
                }
                wasESCPressed = true;
            }
            else
            {
                wasESCPressed = false;
            }

            if (IsActive && !isGamePaused)
            {
                IsMouseVisible = false;
                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            }
            else
            {
                IsMouseVisible = true;
            }

            EffectLoader.SpriteEffect.Parameters["GlobalTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            //EffectLoader.GroundEffect.Parameters["GlobalTime"].SetValue(1);
            //EffectLoader.SpriteEffect.Parameters["GlobalTime"].SetValue(0f);

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            fps.CountFrame(deltaTime);
            playerManager.Update(GraphicsDevice, terrain, (float)deltaTime);
            clock.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            lightManager.Update(clock.DayDelta, deltaTime);
            lightManager.Apply();


            string debugstring = $"Seed: {terrain.seed}," +
                $" Time: {clock.Day:D2}:{clock.Hour:D2}:{clock.Minute:D2}," +
                $" FPS: {fps.FPS}, ms/f: {fps.SPF}";

            renderer.Draw(
                GraphicsDevice,
                terrain,
                objectManager,
                playerManager,
                debugstring,
                isGamePaused);



            base.Draw(gameTime);
        }
    }
}
