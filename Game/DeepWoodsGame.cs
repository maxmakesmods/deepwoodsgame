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

        private int gridSize = 32;
        private int numPatches = 10;

        private Camera camera;
        private Terrain terrain;
        private LightManager lightManager;
        private ObjectManager objectManager;
        private TextHelper textHelper;

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
            IsFixedTimeStep = true;
            _graphics.SynchronizeWithVerticalRetrace = true;
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


            camera = new Camera(GraphicsDevice);
            camera.position = new Vector3(gridSize / 2, 0, gridSize / 2);


            Random rng = new Random();
            terrain = new Terrain(GraphicsDevice, rng.Next(), gridSize, gridSize, numPatches);
            terrain.Apply();

            lightManager = new LightManager(rng.Next(), gridSize, gridSize);
            objectManager = new ObjectManager(rng.Next(), gridSize, gridSize, terrain);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            fps.CountFrame(deltaTime);
            camera.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            lightManager.MoveLightsForFun(deltaTime);
            lightManager.Apply();

            Matrix view = camera.View;
            Matrix projection = camera.Projection;

            terrain.Draw(GraphicsDevice, view, projection);
            objectManager.Draw(GraphicsDevice, view, projection);

            textHelper.DrawStringOnScreen($"Seed: {terrain.seed}, FPS: {fps.FPS}");

            base.Draw(gameTime);
        }
    }
}
