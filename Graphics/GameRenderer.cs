
using DeepWoods.Game;
using DeepWoods.Loaders;
using DeepWoods.Main;
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
    public class GameRenderer
    {
        public static readonly Color ClearColor = Color.Black;

        private readonly DeepWoodsGame game;
        private readonly SpriteBatch spriteBatch;

        public GameRenderer(DeepWoodsGame game)
        {
            this.game = game;
            spriteBatch = new SpriteBatch(DeepWoodsMain.Instance.GraphicsDevice);
        }

        public void Draw(string debugstring, bool isGamePaused)
        {
            foreach (var player in game.PlayerManager.LocalPlayers)
            {
                DrawPlayerScreen(player);
            }

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            DeepWoodsMain.Instance.GraphicsDevice.Clear(ClearColor);
            foreach (var player in game.PlayerManager.LocalPlayers)
            {
                spriteBatch.Draw(player.myRenderTarget, player.PlayerViewport, Color.White);
            }

            DrawPlayerMouseCursors(game.PlayerManager.LocalPlayers, isGamePaused);
            DrawDebugString(debugstring);

            spriteBatch.End();
        }

        private void DrawPlayerScreen(LocalPlayer player)
        {
            game.World.Draw(player);

            foreach (var pl in game.PlayerManager.Players)
            {
                pl.Draw(player.myCamera);
            }

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            player.DrawUI(spriteBatch);
            spriteBatch.End();

            DeepWoodsMain.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawDebugString(string debugstring)
        {
            DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, new Vector2(20f, 20f), debugstring);

            /*
            int i = 0;
            foreach (var player in ATT.PlayerManager.Players)
            {
                spriteBatch.Draw(player.myShadowMap, new Rectangle(32 + (288 * i), 128, 256, 256), Color.White);
                i++;
            }
            */
        }

        private void DrawPlayerMouseCursors(List<LocalPlayer> players, bool isGamePaused)
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

                var terrain = game.World.GetTerrain(player);


                var tilePos = player.myCamera.GetTileAtScreenPos(mouseState.Position);
                var biome = terrain.GetBiome(tilePos.X, tilePos.Y);

                string mouseString = $"{tilePos.X},{tilePos.Y},{biome?.IsVoid??true}";
                DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, new Vector2(mouseState.X, mouseState.Y + 32), mouseString);

                i++;
            }
        }
    }
}
