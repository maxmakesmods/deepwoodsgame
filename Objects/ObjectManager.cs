using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepWoods.Objects
{
    internal class ObjectManager
    {
        private readonly  List<Sprite> sprites;
        private readonly List<DWObject> objectTypes;
        private readonly Random rng;
        private readonly int width;
        private readonly int height;


        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private DynamicVertexBuffer instanceBuffer;

        private struct InstanceData : IVertexType
        {
            public Vector2 WorldPos;
            public Vector4 TexRect;
            public float IsStanding;
            public float IsGlowing;

            public static readonly VertexDeclaration vertexDeclaration = new(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(28, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4)
            );

            public readonly VertexDeclaration VertexDeclaration => vertexDeclaration;
        }

        public ObjectManager(ContentManager content, GraphicsDevice graphicsDevice, int seed, int width, int height, Terrain terrain)
        {
            rng = new Random(seed);
            objectTypes = content.Load<List<DWObject>>("objects/objects");
            sprites = new List<Sprite>();

            this.width = width;
            this.height = height;

            CreateBasicBuffers(graphicsDevice);
            GenerateObjects(terrain);


            InstanceData[] instances = new InstanceData[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                instances[i] = new InstanceData()
                {
                    WorldPos = sprites[i].WorldPos,
                    TexRect = new(sprites[i].TexRect.X, sprites[i].TexRect.Y, sprites[i].TexRect.Width, sprites[i].TexRect.Height),
                    IsStanding = sprites[i].IsStanding ? 1f : 0f,
                    IsGlowing = sprites[i].IsGlowing ? 1f : 0f
                };
            }

            instanceBuffer = new(graphicsDevice, InstanceData.vertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);
        }

        private void GenerateObjects(Terrain terrain)
        {
            // TODO TEMP Sprite Test
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (terrain.tiles[x, y].isOpen)
                    {
                        if (rng.NextSingle() < 0.2f)
                        {
                            var dwobj = objectTypes.Where(o => !o.name.StartsWith("tree")).OrderBy(_ => rng.Next()).FirstOrDefault();
                            sprites.Add(new Sprite(new Vector2(x, y), new Rectangle(dwobj.x, dwobj.y, dwobj.width, dwobj.height), dwobj.standing, dwobj.glowing));
                        }
                    }
                    else
                    {
                        var dwobj = objectTypes.Where(o => o.name.StartsWith("tree")).OrderBy(_ => rng.Next()).FirstOrDefault();
                        sprites.Add(new Sprite(new Vector2(x, y), new Rectangle(dwobj.x, dwobj.y, dwobj.width, dwobj.height), dwobj.standing, dwobj.glowing));
                    }
                }
            }
        }

        private void CreateBasicBuffers(GraphicsDevice graphicsDevice)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1));

            ushort[] indices = [0, 1, 2, 0, 2, 3];

            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }


        internal void DrawShadowMap(GraphicsDevice graphicsDevice, Camera camera)
        {
            Matrix view = camera.ShadowView;
            Matrix projection = camera.ShadowProjection;

            graphicsDevice.SetRenderTarget(TextureLoader.ShadowMap);
            graphicsDevice.Clear(Color.Black);

            var spriteEffect = EffectLoader.SpriteEffect;

            graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(vertexBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1));
            graphicsDevice.Indices = indexBuffer;

            spriteEffect.Parameters["ObjectTextureSize"].SetValue(new Vector2(TextureLoader.ObjectsTexture.Width, TextureLoader.ObjectsTexture.Height));
            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(TextureLoader.ObjectsTexture);

            spriteEffect.Parameters["IsShadow"].SetValue(1);
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, sprites.Count);
            }

            graphicsDevice.SetRenderTarget(null);
        }


        internal void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            Matrix view = camera.View;
            Matrix projection = camera.Projection;

            var spriteEffect = EffectLoader.SpriteEffect;

            graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(vertexBuffer, 0, 0),
                new VertexBufferBinding(instanceBuffer, 0, 1));
            graphicsDevice.Indices = indexBuffer;

            spriteEffect.Parameters["ObjectTextureSize"].SetValue(new Vector2(TextureLoader.ObjectsTexture.Width, TextureLoader.ObjectsTexture.Height));
            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(TextureLoader.ObjectsTexture);
            spriteEffect.Parameters["IsShadow"].SetValue(0);

            spriteEffect.Parameters["ShadowMap"].SetValue(TextureLoader.ShadowMap);
            spriteEffect.Parameters["ShadowMapBounds"].SetValue(camera.ShadowRectangle.GetBoundsV4());
            spriteEffect.Parameters["ShadowMapTileSize"].SetValue(camera.ShadowRectangle.GetSizeV2());

            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, sprites.Count);
            }


        }
    }
}