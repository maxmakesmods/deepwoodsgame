
using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
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
    internal class Player
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



        private static readonly float WalkSpeed = 2f;

        public Vector2 position;
        public Point lookDir;
        public Point lookAt;

        public Camera myCamera;
        public RenderTarget2D myRenderTarget;
        public RenderTarget2D myShadowMap;

        private VertexCharacterData[] vertices;
        private short[] indices;


        private float animationFPS = 8f;
        private int animationFrame = 0;
        private int animationRow = 0;
        private float frameTimeCounter = 0f;
        public PlayerIndex PlayerIndex { get; private set; }
        public Rectangle PlayerViewport { get; private set; }

        private readonly RectangleF relativeViewport;
        private KeyboardState previousKeyboardState;
        private bool noclip;
        private readonly Inventory inventory;

        public Player(GraphicsDevice graphicsDevice, PlayerIndex playerIndex, RectangleF relativeViewport, Vector2 startPos)
        {
            this.relativeViewport = relativeViewport;
            PlayerViewport = relativeViewport.Scale(graphicsDevice.Viewport.Bounds).ToRectangle();

            PlayerIndex = playerIndex;
            position = startPos;
            myCamera = new Camera(graphicsDevice);

            inventory = new Inventory(graphicsDevice);

            myRenderTarget = RecreateRenderTarget(graphicsDevice, SurfaceFormat.Color);
            myShadowMap = RecreateRenderTarget(graphicsDevice, SurfaceFormat.Single);

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

        private RenderTarget2D RecreateRenderTarget(GraphicsDevice graphicsDevice, SurfaceFormat format)
        {
            return new RenderTarget2D(graphicsDevice,
                PlayerViewport.Width, PlayerViewport.Height,
                false,
                format,
                DepthFormat.Depth24,
                0, RenderTargetUsage.DiscardContents, false);

        }

        private Vector4 getTexRect()
        {
            return new Vector4(16 + animationFrame * 64, 16 + animationRow * 64, 32, 32);
        }


        public void Update(AllTheThings att, float timeDelta)
        {
            var keyboardState = DWKeyboard.GetState(PlayerIndex);

            PlayerViewport = relativeViewport.Scale(att.GraphicsDevice.Viewport.Bounds).ToRectangle();
            if (PlayerViewport.Width != myRenderTarget.Width || PlayerViewport.Height != myRenderTarget.Height)
            {
                myRenderTarget.Dispose();
                myRenderTarget = RecreateRenderTarget(att.GraphicsDevice, SurfaceFormat.Color);
            }
            if (PlayerViewport.Width != myRenderTarget.Width || myShadowMap.Height != myShadowMap.Height)
            {
                myShadowMap.Dispose();
                myShadowMap = RecreateRenderTarget(att.GraphicsDevice, SurfaceFormat.Single);
            }

            // get input velocity
            Vector2 velocity = GetVelocity(keyboardState);

            // run animation based on velocity
            animationRow = GetAnimationRow(velocity);
            if (velocity != Vector2.Zero)
            {
                CalculateLookDir(velocity);

                if (keyboardState.IsKeyDown(Keys.LeftShift))
                {
                    frameTimeCounter += timeDelta * 2f;
                }
                else
                {
                    frameTimeCounter += timeDelta;
                }
                if (frameTimeCounter >= 1f / animationFPS)
                {
                    frameTimeCounter = 0f;
                    animationFrame = (animationFrame + 1) % 8;
                }
            }

            // clip velocity against terrain
            if (!noclip)
            {
                velocity = ClipVelocity(att.World.GetTerrain(this), velocity, timeDelta);
            }

            // apply velocity
            SetPosition(position + velocity * timeDelta);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].WorldPos = position;
                vertices[i].TexRect = getTexRect();
            }

            myCamera.Update(position, PlayerViewport, DWMouse.GetState(this), timeDelta);



            if (keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
            {
                if (!DoInteract(att, lookAt))
                {
                    DoInteract(att, position.RoundToPoint());
                }
            }

            if (keyboardState.IsKeyDown(Keys.Tab) && !previousKeyboardState.IsKeyDown(Keys.Tab))
            {
                inventory.IsOpen = !inventory.IsOpen;
            }

            if (keyboardState.IsKeyDown(Keys.K) && !previousKeyboardState.IsKeyDown(Keys.K))
            {
                noclip = !noclip;
                //att.World.SwitchToUnderground(this, 0);
            }

            if (keyboardState.IsKeyDown(Keys.J) && !previousKeyboardState.IsKeyDown(Keys.J))
            {
                //att.World.SwitchToOverground(this);
            }

            if (keyboardState.IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H))
            {
                if (att.DialogueManager.HasOpenDialogue(this))
                {
                    att.DialogueManager.CloseDialogue(this);
                }
                else
                {
                    att.DialogueManager.OpenDialogue(this, new("hello", ["yes", "no", "why even"]));
                }
            }



            previousKeyboardState = keyboardState;
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

        private bool DoInteract(AllTheThings att, Point p)
        {
            if (att.World.IsCave(this, p.X, p.Y))
            {
                att.World.SwitchOverUnderground(this, p.X, p.Y);
                return true;
            }

            var dwobj = att.World.TryPickUpObject(this, p.X, p.Y);
            if (dwobj != null)
            {
                inventory.Add(dwobj);
                return true;
            }

            return false;
        }

        private Vector2 ClipVelocity(Terrain terrain, Vector2 velocity, float timeDelta)
        {
            Vector2 nextPosition = position + velocity * timeDelta;

            int currentTileX = (int)position.X;
            int currentTileY = (int)position.Y;

            if (velocity.X != 0)
            {
                RectangleF nextRectangleX = new RectangleF(nextPosition.X, position.Y, 0.6f, 0.5f);
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int checkX = currentTileX + x;
                        int checkY = currentTileY + y;

                        if (!terrain.CanWalkHere(checkX, checkY))
                        {
                            RectangleF tileRectangle = new RectangleF(checkX, checkY, 0.9f, 0.9f);
                            if (nextRectangleX.Intersects(tileRectangle))
                            {
                                velocity.X = 0;
                            }
                        }
                    }
                }
            }

            if (velocity.Y != 0)
            {
                RectangleF nextRectangleY = new RectangleF(position.X, nextPosition.Y, 0.6f, 0.5f);
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        int checkX = currentTileX + x;
                        int checkY = currentTileY + y;

                        if (!terrain.CanWalkHere(checkX, checkY))
                        {
                            RectangleF tileRectangle = new RectangleF(checkX, checkY, 0.9f, 0.9f);
                            if (nextRectangleY.Intersects(tileRectangle))
                            {
                                velocity.Y = 0;
                            }
                        }
                    }
                }
            }


            return velocity;
        }

        private void CalculateLookDir(Vector2 velocity)
        {
            lookDir = Directions.GetTwoAxisPointFromVector(velocity);
        }

        private int GetAnimationRow(Vector2 velocity)
        {
            return (int)Directions.GetDirFromVector(velocity);
        }

        private Vector2 GetVelocity(KeyboardState keyboardState)
        {
            Vector2 velocity = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W)) velocity.Y += WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.S)) velocity.Y -= WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.A)) velocity.X -= WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.D)) velocity.X += WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.LeftShift)) velocity *= 2f;
            return velocity;
        }

        public void DrawShadow(GraphicsDevice graphicsDevice, Camera camera)
        {
            DoDraw(graphicsDevice, EffectLoader.SpriteEffect, camera.ShadowView, camera.ShadowProjection, true);
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
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

        public void DrawUI(AllTheThings att, SpriteBatch spriteBatch)
        {
            Vector2 screenPosTopLeft = myCamera.GetScreenPosAtTile(lookAt);
            Vector2 screenPosBottomRight = myCamera.GetScreenPosAtTile(lookAt + new Point(1, 1));
            Vector2 screenPosBottomLeft = new Vector2(screenPosTopLeft.X, screenPosBottomRight.Y);
            SizeF screenPosRectangleSize = (screenPosTopLeft - screenPosBottomRight).Abs();
            RectangleF lookAtRectangle = new(screenPosBottomLeft, screenPosRectangleSize);
            spriteBatch.DrawRectangle(lookAtRectangle, Color.DarkOrchid, 3f);

            inventory.DrawUI(att, spriteBatch);
            att.DialogueManager.DrawUI(att, spriteBatch, this);
        }
    }
}
