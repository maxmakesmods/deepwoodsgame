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
        private AllTheThings ATT { get; set; } = new();
        private Random rng = new();

        private int numPlayers = 1;
        private int gridSize = 128;

        private bool wasESCPressed = false;
        private bool isGamePaused = false;

        public DeepWoodsGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            ATT.GraphicsDeviceManager = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            base.Initialize();
            RawInput.Initialize(WindowHelper.GetRealHWNDFromSDL(Window.Handle));
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;



            ATT.GameWindow = Window;
            ATT.Content = Content;
            ATT.GraphicsDevice = GraphicsDevice;
            ATT.FPS = new FPSCounter();
            ATT.Clock = new InGameClock();

            ATT.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            ATT.GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
            ATT.GraphicsDeviceManager.PreferredBackBufferHeight = 1080;
            ATT.GraphicsDeviceManager.ApplyChanges();

            ATT.GraphicsDevice.BlendState = BlendState.Opaque;
            ATT.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ATT.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            ATT.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            EffectLoader.Load(Content);
            TextureLoader.Load(Content, GraphicsDevice);

            ATT.TextHelper = new TextHelper(ATT);
            ATT.PlayerManager = new PlayerManager(ATT, rng.Next());
            ATT.Renderer = new DWRenderer(ATT);
            ATT.DialogueManager = new DialogueManager();

            int terrainSeed = rng.Next();//382081431;

            ATT.World = new GameWorld(ATT, terrainSeed, gridSize, gridSize);



            ATT.Clock.TimeScale = 0;
            ATT.Clock.SetTime(1, 10, 0);

            ATT.PlayerManager.SpawnPlayers(numPlayers);


            // TODO
            GameState.IsMultiplayerGame = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (!wasESCPressed)
                {
                    isGamePaused = !isGamePaused;
                    if (isGamePaused)
                    {
                        MouseState ms = DWMouse.GetState(ATT.PlayerManager.Players[0]);
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

            ATT.FPS.CountFrame(deltaTime);
            ATT.PlayerManager.Update((float)deltaTime);
            ATT.Clock.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ATT.World.Update(ATT.Clock.DayDelta, deltaTime);


            string debugstring = $"Seed: {ATT.World.Seed}," +
                $" Time: {ATT.Clock.Day:D2}:{ATT.Clock.Hour:D2}:{ATT.Clock.Minute:D2}," +
                $" FPS: {ATT.FPS.FPS}, ms/f: {ATT.FPS.SPF}";

            ATT.Renderer.Draw(debugstring, isGamePaused);



            base.Draw(gameTime);
        }
    }
}
