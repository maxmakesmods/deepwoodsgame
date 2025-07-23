
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
        private readonly TextHelper textHelper;
        private readonly SpriteBatch spriteBatch;

        public DWRenderer(GraphicsDevice graphicsDevice, ContentManager content)
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
            textHelper = new TextHelper(graphicsDevice, content);

        }

        public void Draw(
            GraphicsDevice graphicsDevice,
            Terrain terrain,
            ObjectManager objectManager,
            PlayerManager playerManager,
            string debugstring,
            bool isGamePaused)
        {
            foreach (var player in playerManager.Players)
            {
                DrawPlayerScreen(graphicsDevice, terrain, objectManager, playerManager.Players, player.myCamera, player.myRenderTarget);
            }

            spriteBatch.Begin();

            graphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var player in playerManager.Players)
            {
                spriteBatch.Draw(player.myRenderTarget, player.PlayerViewport, Color.White);
            }

            DrawPlayerMouseCursors(playerManager.Players, isGamePaused);
            DrawDebugString(debugstring);

            spriteBatch.End();
        }

        private void DrawPlayerScreen(GraphicsDevice graphicsDevice, Terrain terrain, ObjectManager objectManager, List<Player> players, Camera camera, RenderTarget2D renderTarget)
        {
            objectManager.DrawShadowMap(graphicsDevice, players, camera);

            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.CornflowerBlue);
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            terrain.Draw(graphicsDevice, camera);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            objectManager.Draw(graphicsDevice, camera);

            foreach (var player in players)
            {
                player.Draw(graphicsDevice, camera);
            }
            graphicsDevice.SetRenderTarget(null);
        }

        private void DrawDebugString(string debugstring)
        {
            textHelper.DrawStringOnScreen(spriteBatch, new Vector2(20f, 20f), debugstring);

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
                textHelper.DrawStringOnScreen(spriteBatch, new Vector2(mouseState.X, mouseState.Y + 32), $"{tilePos.X},{tilePos.Y}");

                i++;
            }
        }
    }
}
