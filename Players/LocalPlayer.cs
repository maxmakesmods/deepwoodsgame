using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Main;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;

namespace DeepWoods.Players
{
    public class LocalPlayer : Player
    {
        public Camera myCamera;
        public RenderTarget2D myRenderTarget;
        public RenderTarget2D myShadowMap;

        private RectangleF relativeViewport;
        private KeyboardState previousKeyboardState;

        private bool noclip;

        public PlayerIndex PlayerIndex { get; private set; }
        public Rectangle PlayerViewport { get; private set; }

        public LocalPlayer(DeepWoodsGame game, PlayerIndex playerIndex, Vector2 startPos)
            :base(game, startPos)
        {
            PlayerIndex = playerIndex;
            myCamera = new Camera(DWMouse.GetState(this));
        }

        public void SetPlayerViewport(RectangleF relativeViewport)
        {
            this.relativeViewport = relativeViewport;
            PlayerViewport = relativeViewport.Scale(DeepWoodsMain.Instance.GraphicsDevice.Viewport.Bounds).ToRectangle();
            myRenderTarget = RecreateRenderTarget(SurfaceFormat.Color);
            myShadowMap = RecreateRenderTarget(SurfaceFormat.Single);
        }

        private RenderTarget2D RecreateRenderTarget(SurfaceFormat format)
        {
            return new RenderTarget2D(DeepWoodsMain.Instance.GraphicsDevice,
                PlayerViewport.Width, PlayerViewport.Height,
                false,
                format,
                DepthFormat.Depth24,
                0, RenderTargetUsage.DiscardContents, false);

        }

        private Vector2 GetVelocity(KeyboardState keyboardState, float timeDelta)
        {
            Vector2 velocity = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W)) velocity.Y += WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.S)) velocity.Y -= WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.A)) velocity.X -= WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.D)) velocity.X += WalkSpeed;
            if (keyboardState.IsKeyDown(Keys.LeftShift)) velocity *= 2f;
            // clip velocity against terrain
            if (!noclip)
            {
                velocity = ClipVelocity(game.World.GetTerrain(this), velocity, timeDelta);
            }
            return velocity;
        }


        public override void Update(float timeDelta)
        {
            var keyboardState = DWKeyboard.GetState(PlayerIndex);

            velocity = GetVelocity(keyboardState, timeDelta);
            SetPosition(position + velocity * timeDelta);

            DoRenderUpdate(timeDelta);
            DoInteractions(keyboardState);

            previousKeyboardState = keyboardState;

            base.Update(timeDelta);
        }

        private void DoRenderUpdate(float timeDelta)
        {
            PlayerViewport = relativeViewport.Scale(DeepWoodsMain.Instance.GraphicsDevice.Viewport.Bounds).ToRectangle();
            if (PlayerViewport.Width != myRenderTarget.Width || PlayerViewport.Height != myRenderTarget.Height)
            {
                myRenderTarget.Dispose();
                myRenderTarget = RecreateRenderTarget(SurfaceFormat.Color);
            }
            if (PlayerViewport.Width != myRenderTarget.Width || myShadowMap.Height != myShadowMap.Height)
            {
                myShadowMap.Dispose();
                myShadowMap = RecreateRenderTarget(SurfaceFormat.Single);
            }

            myCamera.Update(position, PlayerViewport, DWMouse.GetState(this), timeDelta);
        }

        private void DoInteractions(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.E) && !previousKeyboardState.IsKeyDown(Keys.E))
            {
                if (!DoInteract(lookAt))
                {
                    DoInteract(position.RoundToPoint());
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
                if (game.DialogueManager.HasOpenDialogue(this))
                {
                    game.DialogueManager.CloseDialogue(this);
                }
                else
                {
                    game.DialogueManager.OpenDialogue(this, new("hello", ["yes", "no", "why even"]));
                }
            }
        }

        private bool DoInteract(Point p)
        {
            if (game.World.IsCave(this, p.X, p.Y))
            {
                game.World.SwitchOverUnderground(this, p.X, p.Y);
                return true;
            }

            var dwobj = game.World.TryPickUpObject(this, p.X, p.Y);
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

        public void DrawUI(SpriteBatch spriteBatch)
        {
            Vector2 screenPosTopLeft = myCamera.GetScreenPosAtTile(lookAt);
            Vector2 screenPosBottomRight = myCamera.GetScreenPosAtTile(lookAt + new Point(1, 1));
            Vector2 screenPosBottomLeft = new Vector2(screenPosTopLeft.X, screenPosBottomRight.Y);
            SizeF screenPosRectangleSize = (screenPosTopLeft - screenPosBottomRight).Abs();
            RectangleF lookAtRectangle = new(screenPosBottomLeft, screenPosRectangleSize);

            lookAtRectangle.X -= myCamera.Viewport.X;
            lookAtRectangle.Y -= myCamera.Viewport.Y;

            spriteBatch.DrawRectangle(lookAtRectangle, Color.DarkOrchid, 3f);

            inventory.DrawUI(spriteBatch);
            game.DialogueManager.DrawUI(spriteBatch, this);
        }
    }
}
