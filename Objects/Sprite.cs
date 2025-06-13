using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.Objects
{
    internal class Sprite
    {
        private Vector2 pos;
        private Texture2D tex;
        private bool standing;

        private VertexPositionTexture[] vertices;
        private short[] indices;

        public Sprite(Texture2D tex, Vector2 pos, Vector2 size, bool standing)
        {
            this.tex = tex;
            this.pos = pos;
            this.standing = standing;

            vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0f, 1f));
            vertices[1] = new VertexPositionTexture(new Vector3(0, size.Y, 0), new Vector2(0f, 0f));
            vertices[2] = new VertexPositionTexture(new Vector3(size.X, size.Y, 0), new Vector2(1f, 0f));
            vertices[3] = new VertexPositionTexture(new Vector3(size.X, 0, 0), new Vector2(1f, 1f));

            indices = [0, 1, 2, 0, 2, 3];
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect spriteEffect, Matrix view, Matrix projection)
        {
            Matrix world;
            if (standing)
            {
                world = Matrix.CreateRotationX(MathHelper.ToRadians(20f)) * Matrix.CreateTranslation(pos.X, pos.Y, 0f);
            }
            else
            {
                world = Matrix.CreateTranslation(pos.X, pos.Y, 0f);
            }

            spriteEffect.Parameters["World"].SetValue(world);
            spriteEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(tex);
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }
    }
}
