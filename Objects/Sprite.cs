using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;

namespace DeepWoods.Objects
{
    internal class Sprite
    {
        private Vector2 pos;
        private Texture2D tex;
        private bool standing;

        private VertexPositionTexture[] vertices;
        private short[] indices;

        public Sprite(Texture2D tex, Vector2 pos, Rectangle rect, bool standing)
        {
            this.tex = tex;
            this.pos = pos;
            this.standing = standing;

            float x = (float)rect.X / tex.Width;
            float y = (float)rect.Y / tex.Height;
            float width = (float)rect.Width / tex.Height;
            float height = (float)rect.Height / tex.Height;

            float obj_width = (float)rect.Width / Terrain.CellSize;
            float obj_height = (float)rect.Height / Terrain.CellSize;

            vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(x, y + height));
            vertices[1] = new VertexPositionTexture(new Vector3(0, obj_height, 0), new Vector2(x, y));
            vertices[2] = new VertexPositionTexture(new Vector3(obj_width, obj_height, 0), new Vector2(x + width, y));
            vertices[3] = new VertexPositionTexture(new Vector3(obj_width, 0, 0), new Vector2(x + width, y + height));

            indices = [0, 1, 2, 0, 2, 3];
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect spriteEffect, Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateTranslation(pos.X, pos.Y, 0f);
            if (standing)
            {
                DoDraw(graphicsDevice, spriteEffect, world, view, projection, true);
                world = Matrix.CreateRotationX(MathHelper.ToRadians(20f)) * world;
            }
            DoDraw(graphicsDevice, spriteEffect, world, view, projection, false);
        }

        private void DoDraw(GraphicsDevice graphicsDevice, Effect spriteEffect, Matrix world, Matrix view, Matrix projection, bool isShadow)
        {
            spriteEffect.Parameters["World"].SetValue(world);
            spriteEffect.Parameters["WorldViewProjection"].SetValue(world * view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(tex);
            spriteEffect.Parameters["IsShadow"].SetValue(isShadow ? 1 : 0);
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }
    }
}
