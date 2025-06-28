using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DeepWoods.Game
{
    public class DeepWoodsGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;

        private int gridSize = 32;
        private int numPatches = 10;

        private Camera camera;
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
        }

        protected override void LoadContent()
        {
            EffectLoader.Load(Content);
            TextureLoader.Load(Content);


            spriteBatch = new SpriteBatch(GraphicsDevice);

            textHelper = new TextHelper(GraphicsDevice, Content);


            clock = new InGameClock();
            //clock.TimeScale = 60;
            clock.SetTime(1, 12, 0);


            camera = new Camera(GraphicsDevice);
            camera.position = new Vector3(gridSize / 2, 0, gridSize / 2);


            Random rng = new Random();
            terrain = new Terrain(GraphicsDevice, rng.Next(), gridSize, gridSize, numPatches);
            terrain.Apply();

            lightManager = new LightManager(rng.Next(), gridSize, gridSize);
            objectManager = new ObjectManager(Content, GraphicsDevice, rng.Next(), gridSize, gridSize, terrain);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (IsActive)
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
            camera.Update((float)deltaTime);
            clock.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            lightManager.Update(clock.DayDelta, deltaTime);
            lightManager.Apply();

            Matrix view = camera.View;
            Matrix projection = camera.Projection;

            terrain.Draw(GraphicsDevice, view, projection);
            objectManager.Draw(GraphicsDevice, view, projection);

            string debugstring = $"Seed: {terrain.seed}," +
                $" Time: {clock.Day}:{clock.Hour}:{clock.Minute},";


            spriteBatch.Begin();

            List<Color> colors = [
                Color.Pink,
                Color.AliceBlue
                ];

            int i = 0;
            foreach (var (_, mousePos) in RawInput.mousePositions)
            {
                spriteBatch.Draw(TextureLoader.MouseCursor,
                    new Rectangle(mousePos.X, mousePos.Y, TextureLoader.MouseCursor.Width * 2, TextureLoader.MouseCursor.Height * 2),
                    colors[i % 2]);

                var tilePos = camera.GetTileAtScreenPos(mousePos);
                debugstring += $" Tile (Player {i+1}): {tilePos.X},{tilePos.Y},";

                i++;
            }

            debugstring += $" FPS: {fps.FPS}, ms/f: {fps.SPF}";

            textHelper.DrawStringOnScreen(spriteBatch, debugstring);


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
