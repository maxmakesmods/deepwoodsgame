using DeepWoods.Game;
using Microsoft.Xna.Framework;
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

        private AllTheThings ATT { get; set; }

        private Random rng;

        public List<Player> Players => players;

        public PlayerManager(AllTheThings att, int seed)
        {
            ATT = att;
            rng = new Random(seed);
        }

        public void SpawnPlayers(int numPlayers)
        {
            int spawnX = ATT.Terrain.Width / 2;
            int spawnY = ATT.Terrain.Height / 2;
            while (!ATT.Terrain.CanSpawnHere(spawnX, spawnY))
            {
                spawnX = rng.Next(ATT.Terrain.Width);
                spawnY = rng.Next(ATT.Terrain.Height);
            }

            for (int i = 0; i < numPlayers; i++)
            {
                players.Add(new Player(ATT.GraphicsDevice, (PlayerIndex)i, playerRectangles[numPlayers - 1][i], new Vector2(spawnX, spawnY)));
            }
        }

        internal void Update(float deltaTime)
        {
            foreach (var player in players)
            {
                player.Update(ATT, (float)deltaTime);
            }
        }
    }
}
