
using DeepWoods.Game;
using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeepWoods.Graphics
{
    internal class DWRenderer
    {
        private AllTheThings ATT { get; set; }
        private readonly SpriteBatch spriteBatch;

        public DWRenderer(AllTheThings att)
        {
            ATT = att;
            spriteBatch = new SpriteBatch(att.GraphicsDevice);
        }

        public void Draw(string debugstring, bool isGamePaused)
        {
            foreach (var player in ATT.PlayerManager.Players)
            {
                DrawPlayerScreen(player);
            }

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            ATT.GraphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var player in ATT.PlayerManager.Players)
            {
                spriteBatch.Draw(player.myRenderTarget, player.PlayerViewport, Color.White);
            }

            DrawPlayerMouseCursors(ATT.PlayerManager.Players, isGamePaused);
            DrawDebugString(debugstring);

            spriteBatch.End();
        }

        private void DrawPlayerScreen(Player player)
        {
            ATT.ObjectManager.DrawShadowMap(ATT.GraphicsDevice, ATT.PlayerManager.Players, player.myCamera);

            ATT.GraphicsDevice.SetRenderTarget(player.myRenderTarget);
            ATT.GraphicsDevice.Clear(Color.CornflowerBlue);
            ATT.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            ATT.Terrain.Draw(ATT.GraphicsDevice, player.myCamera);
            ATT.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ATT.ObjectManager.Draw(ATT.GraphicsDevice, player.myCamera);

            foreach (var pl in ATT.PlayerManager.Players)
            {
                pl.Draw(ATT.GraphicsDevice, player.myCamera);
            }

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            player.DrawUI(ATT, spriteBatch);
            spriteBatch.End();

            ATT.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawDebugString(string debugstring)
        {
            ATT.TextHelper.DrawStringOnScreen(spriteBatch, new Vector2(20f, 20f), debugstring);

            //spriteBatch.Draw(TextureLoader.ShadowMap, new Rectangle(32, 128, 256, 256), Color.White);

        }

        private void DrawPlayerMouseCursors(List<Player> players, bool isGamePaused)
        {
            if (isGamePaused)
                return;

            List<Color> colors = [
                Color.Pink,
                Color.AliceBlue
                ];

            int i = 0;
            foreach (var player in players)
            {
                var mouseState = DWMouse.GetState(player);

                spriteBatch.Draw(TextureLoader.MouseCursor,
                    new Rectangle(mouseState.X, mouseState.Y, TextureLoader.MouseCursor.Width * 2, TextureLoader.MouseCursor.Height * 2),
                    colors[i % 2]);

                var tilePos = player.myCamera.GetTileAtScreenPos(mouseState.Position);
                ATT.TextHelper.DrawStringOnScreen(spriteBatch, new Vector2(mouseState.X, mouseState.Y + 32), $"{tilePos.X},{tilePos.Y}");

                i++;
            }
        }
    }
}
