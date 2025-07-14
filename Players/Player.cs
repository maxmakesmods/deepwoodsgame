
using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Animations;
using System;
using System.Runtime.InteropServices;

namespace DeepWoods.Players
{
    internal class Player
    {
        private static readonly int WALK_ROW_TOP_LEFT = 0;
        private static readonly int WALK_ROW_LEFT = 1;
        private static readonly int WALK_ROW_BOTTOM_LEFT = 2;
        private static readonly int WALK_ROW_BOTTOM = 3;
        private static readonly int WALK_ROW_BOTTOM_RIGHT = 4;
        private static readonly int WALK_ROW_RIGHT = 5;
        private static readonly int WALK_ROW_TOP_RIGHT = 6;
        private static readonly int WALK_ROW_TOP = 7;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct VertexCharacterData : IVertexType
        {
            public Vector4 Position;
            public Vector2 TexCoord;
            public Vector2 WorldPos;
            public Vector4 TexRect;
            public float IsStanding;
            public float IsGlowing;

            public static readonly VertexDeclaration vertexDeclaration = new(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(48, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(52, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4)
            );

            public readonly VertexDeclaration VertexDeclaration => vertexDeclaration;
        }



        private static readonly float WalkSpeed = 2f;

        public Vector2 position;

        public Camera camera;

        private VertexCharacterData[] vertices;
        private short[] indices;

        public Player(GraphicsDevice graphicsDevice, Vector2 startPos)
        {
            position = startPos;
            camera = new Camera(graphicsDevice);

            vertices = new VertexCharacterData[4];

            vertices[0] = new VertexCharacterData()
            {
                Position = new Vector4(0, 0, 0, 1),
                TexCoord = new Vector2(0, 1),
                WorldPos = startPos,
                TexRect = getTexRect(),
                IsStanding = 1f,
                IsGlowing = 0f
            };

            vertices[1] = new VertexCharacterData()
            {
                Position = new Vector4(0, 1, 0, 1),
                TexCoord = new Vector2(0, 0),
                WorldPos = startPos,
                TexRect = getTexRect(),
                IsStanding = 1f,
                IsGlowing = 0f
            };

            vertices[2] = new VertexCharacterData()
            {
                Position = new Vector4(1, 1, 0, 1),
                TexCoord = new Vector2(1, 0),
                WorldPos = startPos,
                TexRect = getTexRect(),
                IsStanding = 1f,
                IsGlowing = 0f
            };

            vertices[3] = new VertexCharacterData()
            {
                Position = new Vector4(1, 0, 0, 1),
                TexCoord = new Vector2(1, 1),
                WorldPos = startPos,
                TexRect = getTexRect(),
                IsStanding = 1f,
                IsGlowing = 0f
            };

            indices = [0, 1, 2, 0, 2, 3];
        }


        float animationFPS = 8f;
        int animationFrame = 0;
        int animationRow = 0;
        float frameTimeCounter = 0f;


        private Vector4 getTexRect()
        {
            return new Vector4(16 + animationFrame * 64, 16 + animationRow * 64, 32, 32);
        }

        public void Update(Terrain terrain, float timeDelta)
        {
            var oldPosition = position;

            Vector2 velocity = getVelocity();


            animationRow = getAnimationRow(velocity);

            if (velocity != Vector2.Zero)
            {
                frameTimeCounter += timeDelta;
                if (frameTimeCounter >= 1f / animationFPS)
                {
                    frameTimeCounter = 0f;
                    animationFrame = (animationFrame + 1) % 8;
                }
            }

            position += velocity * timeDelta;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].WorldPos = position;
                vertices[i].TexRect = getTexRect();
            }

            int x = (int)position.X;
            int y = (int)position.Y;

            if (!terrain.tiles[x, y].isOpen)
            {
                position = oldPosition;
            }

            camera.Update(position, timeDelta);
        }

        private int getAnimationRow(Vector2 velocity)
        {
            if (velocity.X < 0)
            {
                if (velocity.Y < 0)
                {
                    return WALK_ROW_BOTTOM_LEFT;
                }
                else if (velocity.Y > 0)
                {
                    return WALK_ROW_TOP_LEFT;
                }
                else
                {
                    return WALK_ROW_LEFT;
                }
            }
            else if (velocity.X > 0)
            {
                if (velocity.Y < 0)
                {
                    return WALK_ROW_BOTTOM_RIGHT;
                }
                else if (velocity.Y > 0)
                {
                    return WALK_ROW_TOP_RIGHT;
                }
                else
                {
                    return WALK_ROW_RIGHT;
                }
            }
            else if (velocity.Y < 0)
            {
                return WALK_ROW_BOTTOM;
            }
            else if (velocity.Y > 0)
            {
                return WALK_ROW_TOP;
            }
            else
            {
                return WALK_ROW_BOTTOM;
            }
        }

        private Vector2 getVelocity()
        {
            Vector2 velocity = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) velocity.Y += WalkSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) velocity.Y -= WalkSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) velocity.X -= WalkSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) velocity.X += WalkSpeed;
            return velocity;
        }

        public void DrawShadow(GraphicsDevice graphicsDevice)
        {
            DoDraw(graphicsDevice, EffectLoader.SpriteEffect, camera.View, camera.Projection, true);
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            DoDraw(graphicsDevice, EffectLoader.SpriteEffect, camera.View, camera.Projection, false);
        }

        private void DoDraw(GraphicsDevice graphicsDevice, Effect spriteEffect, Matrix view, Matrix projection, bool isShadow)
        {
            spriteEffect.Parameters["ObjectTextureSize"].SetValue(new Vector2(TextureLoader.CharacterTileSet.Width, TextureLoader.CharacterTileSet.Height));
            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(TextureLoader.CharacterTileSet);

            spriteEffect.Parameters["IsShadow"].SetValue(isShadow ? 1 : 0);
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }
    }
}
