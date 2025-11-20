
using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Main;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Runtime.InteropServices;

namespace DeepWoods.Players
{
    public abstract class Player
    {
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



        public static readonly float WalkSpeed = 2f;

        public Vector2 velocity;
        public Vector2 position;
        public Point lookDir;
        public Point lookAt;

        private VertexCharacterData[] vertices;
        private short[] indices;


        private float animationFPS = 8f;
        private int animationFrame = 0;
        private int animationRow = 0;
        private float frameTimeCounter = 0f;

        protected readonly DeepWoodsGame game;
        protected readonly Inventory inventory;

        public PlayerId ID { get; private set; }

        public Player(DeepWoodsGame game, PlayerId id, Vector2 startPos)
        {
            ID = id;
            this.game = game;
            CalculateLookDir(Vector2.Zero);
            SetPosition(startPos);
            inventory = new Inventory();

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

        private Vector4 getTexRect()
        {
            return new Vector4(16 + animationFrame * 64, 16 + animationRow * 64, 32, 32);
        }


        public virtual void Update(float timeDelta)
        {
            UpdateSprite(timeDelta);
        }

        public void UpdateSprite(float timeDelta)
        {
            // run animation based on velocity
            animationRow = GetAnimationRow(velocity);
            if (velocity != Vector2.Zero)
            {
                CalculateLookDir(velocity);
                frameTimeCounter += timeDelta * velocity.Length() / WalkSpeed;
                if (frameTimeCounter >= 1f / animationFPS)
                {
                    frameTimeCounter = 0f;
                    animationFrame = (animationFrame + 1) % 8;
                }
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].WorldPos = position;
                vertices[i].TexRect = getTexRect();
            }
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
            lookAt = position.RoundToPoint() + lookDir;
        }

        public void SetLookDir(Point lookDir)
        {
            this.lookDir = lookDir;
            lookAt = position.RoundToPoint() + lookDir;
        }

        public void SetPosition(Vector2 position, Point lookDir)
        {
            SetPosition(position);
            SetLookDir(lookDir);
        }

        private void CalculateLookDir(Vector2 velocity)
        {
            lookDir = Directions.GetTwoAxisPointFromVector(velocity);
        }

        private int GetAnimationRow(Vector2 velocity)
        {
            return (int)Directions.GetDirFromVector(velocity);
        }

        public void DrawShadow(Camera camera)
        {
            DoDraw(EffectLoader.SpriteEffect, camera.ShadowView, camera.ShadowProjection, true);
        }

        public void Draw(Camera camera)
        {
            DoDraw(EffectLoader.SpriteEffect, camera.View, camera.Projection, false);
        }

        private void DoDraw(Effect spriteEffect, Matrix view, Matrix projection, bool isShadow)
        {
            spriteEffect.Parameters["ObjectTextureSize"].SetValue(new Vector2(TextureLoader.CharacterTileSet.Width, TextureLoader.CharacterTileSet.Height));
            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["SpriteTexture"].SetValue(TextureLoader.CharacterTileSet);

            spriteEffect.Parameters["IsShadow"].SetValue(isShadow ? 1 : 0);
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DeepWoodsMain.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }
    }
}
