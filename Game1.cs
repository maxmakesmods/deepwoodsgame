using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods
{
    public class Game1 : Game
    {
        private Effect groundEffect;
        private Effect spriteEffect;
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

        private int mapSeed;

        private SpriteBatch spriteBatch;
        private SpriteFont ft88RegularFont;

        private FPSCounter drawFPS = new();
        private FPSCounter updateFPS = new();

        private Texture2D treeTexture;
        private Texture2D towerTexture;
        private Texture2D wagonTexture;
        private Texture2D campfireAbandonedTexture;

        private List<Sprite> sprites = new List<Sprite>();

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
            this.IsFixedTimeStep = true;
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
            camera = new Camera();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            ft88RegularFont = Content.Load<SpriteFont>("fonts/FT88-Regular");

            groundEffect = Content.Load<Effect>("effects/GroundEffect");
            spriteEffect = Content.Load<Effect>("effects/SpriteEffect");
            groundTilesTexture = Content.Load<Texture2D>("groundtiles");
            bluenoiseTexture = Content.Load<Texture2D>("bluenoise_rgba");

            treeTexture = Content.Load<Texture2D>("objects/tree");
            towerTexture = Content.Load<Texture2D>("objects/tower");
            wagonTexture = Content.Load<Texture2D>("objects/abandoned_wagon");
            campfireAbandonedTexture = Content.Load<Texture2D>("objects/campfire_abandoned");

            Random rng = new Random();

            mapSeed = rng.Next();

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
            spriteEffect.Parameters["AmbientLightColor"].SetValue(new Vector3(0.3f, 0.3f, 0.4f));
            //groundEffect.Parameters["AmbientLightColor"].SetValue(Vector3.One);


            // TODO TEMP light test
            int numLights = 8;
            for (int i = 0; i < numLights; i++)
            {
                float distance = 0.5f + rng.NextSingle() * 2f;
                Vector3 color = new Vector3(rng.NextSingle(), rng.NextSingle(), rng.NextSingle());
                Vector2 position = new Vector2(rng.NextSingle() * gridSize, rng.NextSingle() * gridSize);
                Vector2 direction = new Vector2(rng.NextSingle() * 2f - 1f, rng.NextSingle() * 2f - 1f);
                float speed = 0.5f + rng.NextSingle() * 2f;

                lights[i] = new Vector4(color.X, color.Y, color.Z, distance);
                lightPositions[i] = position;
                lightDirection[i] = direction;
                lightSpeed[i] = speed;
            }
            groundEffect.Parameters["Lights"].SetValue(lights);
            groundEffect.Parameters["LightPositions"].SetValue(lightPositions);
            groundEffect.Parameters["NumLights"].SetValue(numLights);
            spriteEffect.Parameters["Lights"].SetValue(lights);
            spriteEffect.Parameters["LightPositions"].SetValue(lightPositions);
            spriteEffect.Parameters["NumLights"].SetValue(numLights);




            var terrainGrid = Terrain.GenerateTerrain(mapSeed, gridSize, gridSize, numPatches);
            var terrainGridTexture = Terrain.GenerateTerrainTexture(GraphicsDevice, terrainGrid);

            // TODO TEMP Sprite Test
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = gridSize - 1; y >= 0; y--)
                {
                    if (terrainGrid[x,y] == Terrain.GroundType.Grass)
                    {
                        if (rng.NextSingle() < 0.9f)
                        {
                            sprites.Add(new Sprite(treeTexture, new Vector2(x, y), new Vector2(1, 2), true));
                        }
                        else
                        {
                            if (rng.NextSingle() < 0.5f)
                            {
                                sprites.Add(new Sprite(towerTexture, new Vector2(x, y), new Vector2(1, 2), true));
                            }
                            else
                            {
                                sprites.Add(new Sprite(wagonTexture, new Vector2(x, y), new Vector2(1, 1), true));
                            }
                        }
                    }
                    else
                    {
                        if (rng.NextSingle() < 0.05f)
                        {
                            sprites.Add(new Sprite(campfireAbandonedTexture, new Vector2(x, y), new Vector2(1, 1), false));
                        }
                    }
                }
            }



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
            spriteEffect.Parameters["LightPositions"].SetValue(lightPositions);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            updateFPS.CountFrame(deltaTime);
            camera.Update(deltaTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            drawFPS.CountFrame(deltaTime);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            MoveLightsForFun(deltaTime);

            Matrix world = Matrix.Identity;
            Matrix view = camera.View;
            Matrix projection = camera.Projection;
            groundEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);

            foreach (EffectPass pass in groundEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, drawingQuad, 0, 4, drawingIndices, 0, 2);
            }

            foreach (var sprite in sprites)
            {
                sprite.Draw(GraphicsDevice, spriteEffect, view, projection);
            }

            DrawStringOnScreen($"Seed: {mapSeed}, DrawFPS: {drawFPS.FPS}, UpdateFPS: {updateFPS.FPS}");

            base.Draw(gameTime);
        }

        private void DrawStringOnScreen(string text)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(ft88RegularFont, text, new Vector2(22f, 22f), Color.Black);
            spriteBatch.DrawString(ft88RegularFont, text, new Vector2(20f, 20f), Color.White);
            spriteBatch.End();
        }
    }
}
