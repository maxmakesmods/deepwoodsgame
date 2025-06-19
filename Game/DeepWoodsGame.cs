using System;
using System.Collections.Generic;
using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods.Game
{
    public class DeepWoodsGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;

        private int gridSize = 128;
        private int numPatches = 10;

        private Camera camera;
        private Terrain terrain;
        private LightManager lightManager;
        private ObjectManager objectManager;
        private TextHelper textHelper;
        private InGameClock clock;

        private FPSCounter fps = new();

        public DeepWoodsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
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
            objectManager = new ObjectManager(Content, rng.Next(), gridSize, gridSize, terrain);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
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

            textHelper.DrawStringOnScreen($"Seed: {terrain.seed}, Time: {clock.Day}:{clock.Hour}:{clock.Minute}, FPS: {fps.FPS}, ms/f: {fps.SPF}");

            base.Draw(gameTime);
        }
    }
}
