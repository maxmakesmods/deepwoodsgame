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
        }

        protected override void LoadContent()
        {
            groundEffect = Content.Load<Effect>("GroundEffect");
            SetupBasicEffect();
            SetupUserIndexedVertexRectangle(16f / 9f, 1f, -1f);
        }

        private void SetupBasicEffect()
        {
            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateLookAt(new(0, -1, 2), new(0, 0, 0), Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), 16f/9f, 1f, 1000f);

            groundEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
        }

        private void SetupUserIndexedVertexRectangle(float width, float height, float zpos)
        {
            float halfwidth = width * 0.5f;
            float halfheight = height * 0.5f;
            float bottomscale = 1f;

            drawingQuad = new VertexPositionColorTexture[4];
            drawingQuad[0] = new VertexPositionColorTexture(new Vector3(-halfwidth * bottomscale, -halfheight, zpos), Color.White, new Vector2(0f, 0f));
            drawingQuad[1] = new VertexPositionColorTexture(new Vector3(-halfwidth, halfheight, zpos), Color.Red, new Vector2(0f, 1f));
            drawingQuad[2] = new VertexPositionColorTexture(new Vector3(halfwidth, halfheight, zpos), Color.Green, new Vector2(1f, 1f));
            drawingQuad[3] = new VertexPositionColorTexture(new Vector3(halfwidth * bottomscale, -halfheight, zpos), Color.Blue, new Vector2(1f, 0f));

            drawingIndices = [0, 1, 2, 0, 2, 3];
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (EffectPass pass in groundEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, drawingQuad, 0, 4, drawingIndices, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}
