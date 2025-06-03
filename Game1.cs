using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods
{
    public class Game1 : Game
    {
        private Effect groundEffect;
        private VertexPositionColorTexture[] drawingQuad;
        private short[] drawingIndices;
        private GraphicsDeviceManager _graphics;
        private Texture2D groundTilesTexture;

        private Camera camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.AllowUserResizing = true;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            camera = new Camera();
        }

        protected override void LoadContent()
        {
            groundEffect = Content.Load<Effect>("GroundEffect");
            groundTilesTexture = Content.Load<Texture2D>("groundtiles");

            int gridSize = 8;
            int cellSize = 32;

            SetupUserIndexedVertexRectangle(gridSize, gridSize);
            groundEffect.Parameters["GridSize"].SetValue(new Vector2(gridSize, gridSize));
            groundEffect.Parameters["GroundTilesTexture"].SetValue(groundTilesTexture);
            groundEffect.Parameters["GroundTilesTextureSize"].SetValue(new Vector2(256, 256));
            groundEffect.Parameters["CellSize"].SetValue((float)cellSize);

            var terrainGrid = Terrain.GenerateTerrain(gridSize, gridSize);
            var terrainGridTexture = Terrain.GenerateTerrainTexture(GraphicsDevice, terrainGrid);

            groundEffect.Parameters["TerrainGridTexture"].SetValue(terrainGridTexture);
        }

        private void SetupUserIndexedVertexRectangle(int width, int height)
        {
            drawingQuad = new VertexPositionColorTexture[4];
            drawingQuad[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0f, 0f));
            drawingQuad[1] = new VertexPositionColorTexture(new Vector3(0, height, 0), Color.Red, new Vector2(0f, 1f));
            drawingQuad[2] = new VertexPositionColorTexture(new Vector3(width, height, 0), Color.Green, new Vector2(1f, 1f));
            drawingQuad[3] = new VertexPositionColorTexture(new Vector3(width, 0, 0), Color.Blue, new Vector2(1f, 0f));

            drawingIndices = [0, 1, 2, 0, 2, 3];
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix world = Matrix.Identity;
            Matrix view = camera.View;
            Matrix projection = camera.Projection;
            groundEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);

            foreach (EffectPass pass in groundEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, drawingQuad, 0, 4, drawingIndices, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}
