using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.Players;
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

        private Player player;
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
            TextureLoader.Load(Content, GraphicsDevice);


            spriteBatch = new SpriteBatch(GraphicsDevice);

            textHelper = new TextHelper(GraphicsDevice, Content);


            clock = new InGameClock();
            clock.TimeScale = 0;
            clock.SetTime(1, 10, 0);


            player = new Player(GraphicsDevice, new Vector2(gridSize / 2, gridSize / 2));


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
            player.Update(terrain, (float)deltaTime);
            clock.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            lightManager.Update(clock.DayDelta, deltaTime);
            lightManager.Apply();

            objectManager.DrawShadowMap(GraphicsDevice, player, player.camera);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            terrain.Draw(GraphicsDevice, player.camera);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            objectManager.Draw(GraphicsDevice, player.camera);

            string debugstring = $"Seed: {terrain.seed}," +
                $" Time: {clock.Day}:{clock.Hour}:{clock.Minute},";


            spriteBatch.Begin();


            player.Draw(GraphicsDevice);


            //spriteBatch.Draw(TextureLoader.ShadowMap, new Rectangle(32, 128, 256, 256), Color.White);



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

                var tilePos = player.camera.GetTileAtScreenPos(mousePos);
                debugstring += $" Tile (Player {i+1}): {tilePos.X},{tilePos.Y},";

                i++;
            }

            debugstring += $" FPS: {fps.FPS}, ms/f: {fps.SPF}\nShadowRect: {player.camera.ShadowRectangle}";

           

            textHelper.DrawStringOnScreen(spriteBatch, debugstring);


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
