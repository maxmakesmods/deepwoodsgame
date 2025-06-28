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

        private float obj_width;
        private float obj_height;
        private float tex_x;
        private float tex_y;
        private float tex_width;
        private float tex_height;

        public Matrix World { get; private set; }

        public Vector4 TexRect => new(tex_x, tex_y, tex_width, tex_height);
        public Vector2 TileSize => new(obj_width, obj_height);


        public Sprite(Texture2D tex, Vector2 pos, Rectangle rect, bool standing)
        {
            this.tex = tex;
            this.pos = pos;
            this.standing = standing;

            tex_x = (float)rect.X / tex.Width;
            tex_y = (float)rect.Y / tex.Height;
            tex_width = (float)rect.Width / tex.Height;
            tex_height = (float)rect.Height / tex.Height;

            obj_width = (float)rect.Width / Terrain.CellSize;
            obj_height = (float)rect.Height / Terrain.CellSize;

            vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1));

            indices = [0, 1, 2, 0, 2, 3];

            World = Matrix.CreateRotationX(MathHelper.ToRadians(20f)) * Matrix.CreateTranslation(pos.X, pos.Y, 0f);
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

            spriteEffect.Parameters["obj_width"].SetValue(obj_width);
            spriteEffect.Parameters["obj_height"].SetValue(obj_height);
            spriteEffect.Parameters["tex_x"].SetValue(tex_x);
            spriteEffect.Parameters["tex_y"].SetValue(tex_y);
            spriteEffect.Parameters["tex_width"].SetValue(tex_width);
            spriteEffect.Parameters["tex_height"].SetValue(tex_height);


            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }
    }
}
