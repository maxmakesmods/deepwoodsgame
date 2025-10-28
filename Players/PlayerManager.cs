using DeepWoods.Game;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace DeepWoods.Players
{
    public class PlayerManager
    {
        private List<List<RectangleF>> playerRectangles = [
            [new(0f, 0f, 1f, 1f)],
            [new(0f, 0f, 0.5f, 1f), new (0.5f, 0f, 0.5f, 1f)],
            [new(0f, 0f, 1f, 0.5f), new (0f, 0.5f, 0.5f, 0.5f), new (0.5f, 0.5f, 0.5f, 0.5f)],
            [new(0f, 0f, 0.5f, 0.5f), new(0.5f, 0f, 0.5f, 0.5f), new(0f, 0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f, 0.5f)]
        ];

        private readonly List<Player> players = [];
        private readonly DeepWoodsGame game;
        private readonly Random rng;

        public List<Player> Players => players;

        public PlayerManager(DeepWoodsGame game, int seed)
        {
            this.game = game;
            rng = new Random(seed);
        }

        public void SpawnPlayers(int numPlayers)
        {
            Point spawnPos = game.World.GetSpawnPosition();
            for (int i = 0; i < numPlayers; i++)
            {
                players.Add(new Player(game, (PlayerIndex)i, playerRectangles[numPlayers - 1][i], new Vector2(spawnPos.X, spawnPos.Y)));
            }
        }

        internal void Update(float deltaTime)
        {
            foreach (var player in players)
            {
                player.Update((float)deltaTime);
            }
        }
    }
}
