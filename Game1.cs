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
        private Texture2D bluenoiseTexture;

        private Camera camera;

        private Vector4[] lights = new Vector4[8];
        private Vector2[] lightPositions = new Vector2[8];
        private Vector2[] lightDirection = new Vector2[8];
        private float[] lightSpeed = new float[8];

        private int gridSize = 32;
        private int cellSize = 32;
        private int numPatches = 10;
        private int ditherSize = 2;

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
            camera = new Camera();

            groundEffect = Content.Load<Effect>("GroundEffect");
            groundTilesTexture = Content.Load<Texture2D>("groundtiles");
            bluenoiseTexture = Content.Load<Texture2D>("bluenoise_rgba");

            Random rng = new Random();

            int seed = rng.Next();

            SetupUserIndexedVertexRectangle(gridSize, gridSize);
            groundEffect.Parameters["GridSize"].SetValue(new Vector2(gridSize, gridSize));
            groundEffect.Parameters["GroundTilesTexture"].SetValue(groundTilesTexture);
            groundEffect.Parameters["GroundTilesTextureSize"].SetValue(new Vector2(groundTilesTexture.Width, groundTilesTexture.Height));
            groundEffect.Parameters["CellSize"].SetValue((float)cellSize);

            groundEffect.Parameters["BlueNoiseTexture"].SetValue(bluenoiseTexture);
            groundEffect.Parameters["BlueNoiseDitherChannel"].SetValue(rng.Next(4));
            groundEffect.Parameters["BlueNoiseDitherOffset"].SetValue(new Vector2(rng.Next(bluenoiseTexture.Width), rng.Next(bluenoiseTexture.Height)));
            groundEffect.Parameters["BlueNoiseVariantChannel"].SetValue(rng.Next(4));
            groundEffect.Parameters["BlueNoiseVariantOffset"].SetValue(new Vector2(rng.Next(bluenoiseTexture.Width), rng.Next(bluenoiseTexture.Height)));
            groundEffect.Parameters["BlueNoiseTextureSize"].SetValue(new Vector2(bluenoiseTexture.Width, bluenoiseTexture.Height));
            groundEffect.Parameters["BlurHalfSize"].SetValue(ditherSize);

            groundEffect.Parameters["AmbientLightColor"].SetValue(new Vector3(0.3f, 0.3f, 0.4f));


            // TODO TEMP light test
            for (int i = 0; i < 8; i++)
            {
                float distance = 0.5f + rng.NextSingle() * 2f;
                Vector3 color = new Vector3(rng.NextSingle(), rng.NextSingle(), rng.NextSingle());
                Vector2 position = new Vector2(rng.NextSingle() * gridSize, rng.NextSingle() * gridSize);
                Vector2 direction = new Vector2(rng.NextSingle(), rng.NextSingle());
                float speed = 0.5f + rng.NextSingle() * 2f;

                lights[i] = new Vector4(color.X, color.Y, color.Z, distance);
                lightPositions[i] = position;
                lightDirection[i] = direction;
                lightSpeed[i] = speed;
            }
            groundEffect.Parameters["Lights"].SetValue(lights);
            groundEffect.Parameters["LightPositions"].SetValue(lightPositions);



            var terrainGrid = Terrain.GenerateTerrain(seed, gridSize, gridSize, numPatches);
            var terrainGridTexture = Terrain.GenerateTerrainTexture(GraphicsDevice, terrainGrid);

            groundEffect.Parameters["TerrainGridTexture"].SetValue(terrainGridTexture);

            camera.position = new Vector3(gridSize / 2, 0, gridSize / 2);
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

        private void MoveLightsForFun(float timeDelta)
        {
            for (int i = 0; i < 8; i++)
            {
                lightPositions[i] += lightDirection[i] * lightSpeed[i] * timeDelta;
                if (lightPositions[i].X < 0)
                {
                    lightPositions[i].X = gridSize;
                }
                else if (lightPositions[i].X > gridSize)
                {
                    lightPositions[i].X = 0;
                }
                if (lightPositions[i].Y < 0)
                {
                    lightPositions[i].Y = gridSize;
                }
                else if (lightPositions[i].Y > gridSize)
                {
                    lightPositions[i].Y = 0;
                }
            }
            groundEffect.Parameters["LightPositions"].SetValue(lightPositions);
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

            MoveLightsForFun((float)gameTime.ElapsedGameTime.TotalSeconds);

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
