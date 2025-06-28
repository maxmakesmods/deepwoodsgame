using DeepWoods.Loaders;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
            public Vector4 WorldRow1;
            public Vector4 WorldRow2;
            public Vector4 WorldRow3;
            public Vector4 WorldRow4;
            public Vector4 TexRect;
            public Vector2 TileSize;

            public static readonly VertexDeclaration vertexDeclaration = new(
                // world matrix
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                // tex rect
                new VertexElement(64, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
                // tile size
                new VertexElement(80, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 6)
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
                Matrix world = sprites[i].World;
                instances[i] = new InstanceData()
                {
                    WorldRow1 = new(world.M11, world.M12, world.M13, world.M14),
                    WorldRow2 = new(world.M21, world.M22, world.M23, world.M24),
                    WorldRow3 = new(world.M31, world.M32, world.M33, world.M34),
                    WorldRow4 = new(world.M41, world.M42, world.M43, world.M44),
                    TexRect = sprites[i].TexRect,
                    TileSize = sprites[i].TileSize
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
                            var dwobj = objectTypes.Where(o => o.name != "tree").OrderBy(_ => rng.Next()).FirstOrDefault();
                            sprites.Add(new Sprite(TextureLoader.ObjectsTexture, new Vector2(x, y), new Rectangle(dwobj.x, dwobj.y, dwobj.width, dwobj.height), true));
                        }
                    }
                    else
                    {
                        //var dwobj = objectTypes.Where(o => o.name == "tree").FirstOrDefault();
                        //sprites.Add(new Sprite(TextureLoader.ObjectsTexture, new Vector2(x, y), new Rectangle(dwobj.x, dwobj.y, dwobj.width, dwobj.height), true));
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


        internal void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            var spriteEffect = EffectLoader.SpriteEffect;

            graphicsDevice.SetVertexBuffers(
                new VertexBufferBinding(vertexBuffer),
                new VertexBufferBinding(instanceBuffer));
            graphicsDevice.Indices = indexBuffer;

            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(TextureLoader.ObjectsTexture);
            spriteEffect.Parameters["IsShadow"].SetValue(0);
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, sprites.Count);
            }
        }
    }
}