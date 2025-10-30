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

        private readonly List<RemotePlayer> remotePlayers = [];
        private readonly List<LocalPlayer> localPlayers = [];
        private readonly List<Player> allPlayers = [];
        private readonly DeepWoodsGame game;

        public List<Player> Players => allPlayers;

        public List<LocalPlayer> LocalPlayers => localPlayers;

        public List<RemotePlayer> RemotePlayers => remotePlayers;

        public PlayerManager(DeepWoodsGame game)
        {
            this.game = game;
        }

        public bool SpawnLocalPlayer()
        {
            if (localPlayers.Count >= playerRectangles.Count)
            {
                return false;
            }

            Point spawnPos = game.World.GetSpawnPosition();
            PlayerIndex playerIndex = (PlayerIndex)localPlayers.Count;

            var player = new LocalPlayer(game, playerIndex, new Vector2(spawnPos.X, spawnPos.Y));
            allPlayers.Add(player);
            localPlayers.Add(player);

            for (int i = 0; i < localPlayers.Count; i++)
            {
                localPlayers[i].SetPlayerViewport(playerRectangles[localPlayers.Count - 1][i]);
            }

            return true;
        }

        public void SpawnRemotePlayer()
        {
            Point spawnPos = game.World.GetSpawnPosition();
            var player = new RemotePlayer(game, new Vector2(spawnPos.X, spawnPos.Y));
            allPlayers.Add(player);
            remotePlayers.Add(player);
        }

        internal void Update(float deltaTime)
        {
            foreach (var player in Players)
            {
                player.Update((float)deltaTime);
            }
        }
    }
}
