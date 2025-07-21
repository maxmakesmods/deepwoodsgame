using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace DeepWoods.Game
{
    public class DeepWoodsGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;

        private int gridSize = 128;
        private int numPatches = 10;

        private List<Player> players;
        private Terrain terrain;
        private LightManager lightManager;
        private ObjectManager objectManager;
        private TextHelper textHelper;
        private InGameClock clock;
        private SpriteBatch spriteBatch;

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


            spriteBatch = new SpriteBatch(GraphicsDevice);

            textHelper = new TextHelper(GraphicsDevice, Content);


            clock = new InGameClock();
            clock.TimeScale = 120;
            clock.SetTime(1, 10, 0);


            Random rng = new Random();
            terrain = new Terrain(GraphicsDevice, rng.Next(), gridSize, gridSize, numPatches);
            terrain.Apply();

            lightManager = new LightManager(rng.Next(), gridSize, gridSize);
            objectManager = new ObjectManager(Content, GraphicsDevice, rng.Next(), gridSize, gridSize, terrain);

            int spawnX = gridSize / 2;
            int spawnY = gridSize / 2;
            while (!terrain.tiles[spawnX, spawnY].isOpen)
            {
                spawnX = rng.Next(gridSize);
                spawnY = rng.Next(gridSize);
            }

            players = [
                new Player(GraphicsDevice, PlayerIndex.One, new Vector2(spawnX, spawnY)),
                new Player(GraphicsDevice, PlayerIndex.Two, new Vector2(spawnX, spawnY))
                ];
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

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            fps.CountFrame(deltaTime);
            foreach (var player in players)
            {
                player.Update(terrain, (float)deltaTime);
            }
            clock.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            lightManager.Update(clock.DayDelta, deltaTime);
            lightManager.Apply();


            foreach (var player in players)
            {
                DrawPlayerScreen(player.myCamera, player.myRenderTarget);
            }


            spriteBatch.Begin();

            GraphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var player in players)
            {
                int halfwidth = GraphicsDevice.Viewport.Width / 2;
                int height = GraphicsDevice.Viewport.Height;
                spriteBatch.Draw(player.myRenderTarget, new Rectangle(halfwidth * (int)player.PlayerIndex, 0, halfwidth, height), Color.White);
            }


            DrawDebugInfo();

            spriteBatch.End();


            base.Draw(gameTime);
        }

        private void DrawPlayerScreen(Camera camera, RenderTarget2D renderTarget)
        {
            objectManager.DrawShadowMap(GraphicsDevice, players, camera);

            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            terrain.Draw(GraphicsDevice, camera);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            objectManager.Draw(GraphicsDevice, camera);

            foreach (var player in players)
            {
                player.Draw(GraphicsDevice, camera);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawDebugInfo()
        {
            string debugstring = $"Seed: {terrain.seed}," +
                $" Time: {clock.Day}:{clock.Hour}:{clock.Minute},";

            //spriteBatch.Draw(TextureLoader.ShadowMap, new Rectangle(32, 128, 256, 256), Color.White);

            List<Color> colors = [
                Color.Pink,
                Color.AliceBlue
                ];

            int i = 0;
            foreach (var player in players)
            {
                var mouseState = DWMouse.GetState(player.PlayerIndex);

                spriteBatch.Draw(TextureLoader.MouseCursor,
                    new Rectangle(mouseState.X, mouseState.Y, TextureLoader.MouseCursor.Width * 2, TextureLoader.MouseCursor.Height * 2),
                    colors[i % 2]);

                var tilePos = player.myCamera.GetTileAtScreenPos(mouseState.Position);
                debugstring += $" Tile (Player {i + 1}): {tilePos.X},{tilePos.Y},";

                i++;
            }

            debugstring += $" FPS: {fps.FPS}, ms/f: {fps.SPF}";

            textHelper.DrawStringOnScreen(spriteBatch, debugstring);
        }
    }
}
