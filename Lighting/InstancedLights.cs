using DeepWoods.Loaders;
using DeepWoods.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DeepWoods.Lighting
{
    public class InstancedLights
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private DynamicVertexBuffer instanceBuffer;
        private InstanceData[] instances;
        private readonly Random rng;

        private struct InstanceData : IVertexType
        {
            public Vector2 WorldPos;
            public Vector4 LightColor;
            public float LightAnim;
            public Vector4 Rand;
            public float IsHidden;

            public static readonly VertexDeclaration vertexDeclaration = new(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(28, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(44, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 5)
            );

            public readonly VertexDeclaration VertexDeclaration => vertexDeclaration;
        }

        public InstancedLights(int seed, List<Light> lights)
        {
            rng = new(seed);
            CreateBasicBuffers();
            CreateInstanceBuffer(lights);
        }

        private void CreateInstanceBuffer(List<Light> lights)
        {
            instances = new InstanceData[lights.Count];
            for (int i = 0; i < lights.Count; i++)
            {
                instances[i] = new InstanceData()
                {
                    WorldPos = lights[i].position,
                    LightColor = lights[i].color,
                    LightAnim = 0f,
                    Rand = new(rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), rng.NextSingle()),
                    IsHidden = 0f
                };
            }
            instanceBuffer = new(DeepWoodsMain.Instance.GraphicsDevice, InstanceData.vertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);
        }

        private void CreateBasicBuffers()
        {
            VertexPositionTexture[] vertices =
            [
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 1)),
            ];
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
            foreach (EffectPass pass in EffectLoader.LightEffect.CurrentTechnique.Passes)
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

        internal void UpdateLights(List<Light> lights)
        {
            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].WorldPos = lights[i].position;
                instances[i].LightColor = lights[i].color;
                instances[i].LightAnim = 0f;
                instances[i].IsHidden = 0f;
            }
            instanceBuffer.SetData(instances);
        }
    }
}
