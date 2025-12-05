using DeepWoods.Loaders;
using DeepWoods.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeepWoods.Objects
{
    internal class InstancedObjects
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private DynamicVertexBuffer instanceBuffer;
        private InstanceData[] instances;
        private Texture2D texture;
        private Texture2D glowmap;

        private struct InstanceData : IVertexType
        {
            public Vector2 WorldPos;
            public Vector4 TexRect;
            public float IsStanding;
            public float IsGlowing;
            public Vector3 AnimationData;
            public float ShaderAnim;
            public float IsHidden;

            public static readonly VertexDeclaration vertexDeclaration = new(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(28, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 5),
                new VertexElement(44, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 6),
                new VertexElement(48, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 7)
            );

            public readonly VertexDeclaration VertexDeclaration => vertexDeclaration;
        }

        public InstancedObjects(List<DWObject> sprites, Texture2D texture, Texture2D glowmap)
        {
            this.texture = texture;
            CreateBasicBuffers();
            CreateInstanceBuffer(sprites);
            this.glowmap = glowmap;
        }

        private void CreateInstanceBuffer(List<DWObject> sprites)
        {
            instances = new InstanceData[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                instances[i] = new InstanceData()
                {
                    WorldPos = sprites[i].WorldPos,
                    TexRect = new(sprites[i].TexRect.X, sprites[i].TexRect.Y, sprites[i].TexRect.Width, sprites[i].TexRect.Height),
                    IsStanding = sprites[i].Def.Standing ? 1f : 0f,
                    IsGlowing = sprites[i].Def.Glowing ? 1f : 0f,
                    AnimationData = new(sprites[i].Def.AnimationFrames, sprites[i].Def.AnimationFrameOffset, sprites[i].Def.AnimationFPS),
                    ShaderAnim = (int)sprites[i].Def.ShaderAnim,
                    IsHidden = 0f
                };
            }
            instanceBuffer = new(DeepWoodsMain.Instance.GraphicsDevice, InstanceData.vertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);
        }

        private void CreateBasicBuffers()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1));

            ushort[] indices = [0, 1, 2, 0, 2, 3];

            vertexBuffer = new VertexBuffer(DeepWoodsMain.Instance.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            indexBuffer = new IndexBuffer(DeepWoodsMain.Instance.GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public void Draw()
        {
            DeepWoodsMain.Instance.GraphicsDevice.SetVertexBuffers(new VertexBufferBinding(vertexBuffer, 0, 0), new VertexBufferBinding(instanceBuffer, 0, 1));
            DeepWoodsMain.Instance.GraphicsDevice.Indices = indexBuffer;
            EffectLoader.SpriteEffect.Parameters["ObjectTextureSize"].SetValue(new Vector2(texture.Width, texture.Height));
            EffectLoader.SpriteEffect.Parameters["SpriteTexture"].SetValue(texture);
            EffectLoader.SpriteEffect.Parameters["GlowMap"].SetValue(glowmap);
            foreach (EffectPass pass in EffectLoader.SpriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DeepWoodsMain.Instance.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, instances.Length);
            }
        }

        public void HideInstance(int index)
        {
            instances[index].IsHidden = 1f;
            instanceBuffer.SetData(instances);
        }
    }
}
