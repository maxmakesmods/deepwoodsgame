using DeepWoods.Game;
using DeepWoods.Network.Data;
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

        private readonly Dictionary<Guid, Player> playerMap = [];
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

        public LocalPlayer SpawnLocalPlayer()
        {
            Point spawnPos = game.World.GetSpawnPosition();
            PlayerId playerId = localPlayers.Count == 0 ? PlayerId.HostId : new PlayerId();
            return SpawnLocalPlayer(playerId, new Vector2(spawnPos.X, spawnPos.Y));
        }

        public LocalPlayer SpawnLocalPlayer(PlayerId id, Vector2 spawnPos)
        {
            if (localPlayers.Count >= playerRectangles.Count)
            {
                return null;
            }

            PlayerIndex playerIndex = (PlayerIndex)localPlayers.Count;

            var player = new LocalPlayer(game, playerIndex, id, spawnPos);
            allPlayers.Add(player);
            localPlayers.Add(player);
            playerMap.Add(id.id, player);

            for (int i = 0; i < localPlayers.Count; i++)
            {
                localPlayers[i].SetPlayerViewport(playerRectangles[localPlayers.Count - 1][i]);
            }

            return player;
        }

        public RemotePlayer SpawnRemotePlayer()
        {
            Point spawnPos = game.World.GetSpawnPosition();
            return SpawnRemotePlayer(new PlayerId(), new Vector2(spawnPos.X, spawnPos.Y));
        }

        public RemotePlayer SpawnRemotePlayer(PlayerId id, Vector2 spawnPos)
        {
            var player = new RemotePlayer(game, id, spawnPos);
            allPlayers.Add(player);
            remotePlayers.Add(player);
            playerMap.Add(id.id, player);
            return player;
        }

        internal void Update(float deltaTime)
        {
            foreach (var player in Players)
            {
                player.Update((float)deltaTime);
            }
        }

        internal void UpdatePlayer(PlayerData playerData)
        {
            if (playerData.Id == LocalPlayers[0].ID.id)
            {
                return;
            }

            if (playerMap.TryGetValue(playerData.Id, out var player))
            {
                player.SetPosition(new(playerData.X, playerData.Y));
            }
        }
    }
}
