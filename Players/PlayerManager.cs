using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace DeepWoods.Players
{
    internal class PlayerManager
    {
        private List<List<RectangleF>> playerRectangles = [
            [new(0f, 0f, 1f, 1f)],
            [new(0f, 0f, 0.5f, 1f), new (0.5f, 0f, 0.5f, 1f)],
            [new(0f, 0f, 1f, 0.5f), new (0f, 0.5f, 0.5f, 0.5f), new (0.5f, 0.5f, 0.5f, 0.5f)],
            [new(0f, 0f, 0.5f, 0.5f), new(0.5f, 0f, 0.5f, 0.5f), new(0f, 0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f, 0.5f)]
        ];

        private List<Player> players = new();
        private Random rng;

        public List<Player> Players => players;

        public PlayerManager(int seed)
        {
            rng = new Random(seed);
        }

        public void SpawnPlayers(GraphicsDevice graphicsDevice, Terrain terrain, int numPlayers)
        {
            int spawnX = terrain.terrainGrid.GetLength(0) / 2;
            int spawnY = terrain.terrainGrid.GetLength(1) / 2;
            while (!terrain.tiles[spawnX, spawnY].isOpen)
            {
                spawnX = rng.Next(terrain.terrainGrid.GetLength(0));
                spawnY = rng.Next(terrain.terrainGrid.GetLength(1));
            }

            for (int i = 0; i < numPlayers; i++)
            {
                players.Add(new Player(graphicsDevice, (PlayerIndex)i, playerRectangles[numPlayers - 1][i], new Vector2(spawnX, spawnY)));
            }
        }

        internal void Update(GraphicsDevice graphicsDevice, Terrain terrain, float deltaTime)
        {
            foreach (var player in players)
            {
                player.Update(graphicsDevice, terrain, (float)deltaTime);
            }
        }
    }
}
