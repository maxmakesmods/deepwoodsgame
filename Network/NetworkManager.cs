using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Main;
using DeepWoods.Network.Data;
using DeepWoods.Players;
using System;
using System.Diagnostics;
using System.Linq;

namespace DeepWoods.Network
{
    public class NetworkManager
    {
        private static readonly string Magic = "_DW_";                 // 4 bytes: _DW_
        private static readonly string GameVersion = "0.0.1-alpha";    // 3 bytes: 0.0.1

        private static readonly string InviteCode = "ABCDE";


        private static long NetworkMSperFrame = 33; // 30fps


        private readonly NetworkInterface network;

        private readonly Stopwatch stopwatch;
        private long nextUpdateMiliseconds;


        private NetworkManager(NetworkInterface network)
        {
            this.network = network;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            network.Init(ReceiveMessage, AcceptPlayer, PlayerConnected, PlayerDisconnected);
            bool result = network.StartHost();
            Debug.WriteLine($"NetworkManager.StartHost(): {result}");
        }

        private NetworkManager(NetworkInterface network, string host)
        {
            this.network = network;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            network.Init(ReceiveMessage, AcceptPlayer, PlayerConnected, PlayerDisconnected);
            ConnectionPayload payload = new()
            {
                Magic = Magic,
                GameVersion = GameVersion,
                InviteCode = InviteCode,
                PlayerId = 0
            };
            byte[] data = payload.ToBytes();
            bool result = network.StartClient(host, data, data.Length);
            Debug.WriteLine($"NetworkManager.StartClient(): {result}");
        }

        public static NetworkManager StartHost(NetworkInterface network)
        {
            return new NetworkManager(network);
        }

        public static NetworkManager StartClient(NetworkInterface network, string host)
        {
            return new NetworkManager(network, host);
        }

        public void Update()
        {
            if (stopwatch.ElapsedMilliseconds < nextUpdateMiliseconds)
            {
                return;
            }

            long currentMiliseconds = stopwatch.ElapsedMilliseconds;

            network.Update();

            // TODO: Send messages
            if (network.Mode == NetworkInterface.NetworkMode.Host)
            {
                SendMessageToAll(Message.Create(MessageType.PlayerUpdate, new PlayersUpdateData()
                {
                    Players = DeepWoodsMain.Instance.Game.PlayerManager.Players.Select(p => new PlayerData()
                    {
                        Id = p.ID.id,
                        X = p.position.X,
                        Y = p.position.Y
                    }).ToList()
                }));
            }
            else if (network.Mode == NetworkInterface.NetworkMode.Client)
            {
                if (DeepWoodsMain.Instance.Game.PlayerManager.LocalPlayers.Count > 0)
                {
                    SendMessage(PlayerId.HostId, Message.Create(MessageType.PlayerUpdate, new PlayersUpdateData()
                    {
                        Players =
                        [
                            new()
                            {
                                Id = DeepWoodsMain.Instance.Game.PlayerManager.LocalPlayers[0].ID.id,
                                X = DeepWoodsMain.Instance.Game.PlayerManager.LocalPlayers[0].position.X,
                                Y = DeepWoodsMain.Instance.Game.PlayerManager.LocalPlayers[0].position.Y
                            }
                        ]
                    }));
                }
            }

            nextUpdateMiliseconds = currentMiliseconds + NetworkMSperFrame;
        }

        private bool AcceptPlayer(byte[] data, int dataOffset, int dataSize, out PlayerId playerId)
        {
            Debug.WriteLine($"NetworkManager.AcceptPlayer()!");
            playerId = default;
            ConnectionPayload payload = data.FromBytes<ConnectionPayload>(dataOffset, dataSize);
            if (payload.Magic != Magic)
            {
                Debug.WriteLine($"payload.magic != Magic: {payload.Magic}");
                return false;
            }
            if (payload.GameVersion != GameVersion)
            {
                Debug.WriteLine($"payload.gameVersion != GameVersion: {payload.GameVersion}");
                return false;
            }
            if (payload.InviteCode != InviteCode)
            {
                Debug.WriteLine($"payload.inviteCode != InviteCode: {payload.InviteCode}");
                return false;
            }

            var player = DeepWoodsMain.Instance.Game.PlayerManager.SpawnRemotePlayer();
            playerId = player.ID;

            bool result = SendMessageToAll(Message.Create(MessageType.PlayerAdded, new PlayerData()
            {
                Id = playerId.id,
                X = player.position.X,
                Y = player.position.Y
            }));

            Debug.WriteLine($"AcceptPlayer: {playerId.id}, {result}");
            return true;
        }

        private bool ReceiveMessage(PlayerId sender, byte[] data, int dataOffset, int dataSize)
        {
            Debug.WriteLine($"ReceiveMessage!");
            Message msg = data.FromBytes<Message>(dataOffset, dataSize);

            if (msg == null)
            {
                Debug.WriteLine($"ReceiveMessage: msg == null!");
                return false;
            }

            switch (msg.Type)
            {
                case MessageType.Welcome:
                    return ProcessWelcomeMessage(sender, msg.GetPayload<WelcomeWorldData>());
                case MessageType.PlayerAdded:
                    return ProcessPlayerChanged(sender, msg.GetPayload<PlayerData>(), true);
                case MessageType.PlayerRemoved:
                    return ProcessPlayerChanged(sender, msg.GetPayload<PlayerData>(), false);
                case MessageType.PlayerUpdate:
                    return ProcessPlayerUpdate(sender, msg.GetPayload<PlayersUpdateData>());
                default:
                    Debug.WriteLine($"ReceiveMessage: invalid type: {msg.Type}");
                    return false;
            }
        }

        private bool ProcessPlayerChanged(PlayerId sender, PlayerData playerData, bool added)
        {
            if (network.Mode != NetworkInterface.NetworkMode.Client)
            {
                return false;
            }

            if (DeepWoodsMain.Instance.Game.World == null)
            {
                return false;
            }

            if (added)
            {
                DeepWoodsMain.Instance.Game.PlayerManager.SpawnRemotePlayer(new(playerData.Id), new(playerData.X, playerData.Y));
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ProcessPlayerUpdate(PlayerId sender, PlayersUpdateData playerData)
        {
            if (network.Mode == NetworkInterface.NetworkMode.Host
                && (playerData.Players.Count != 1 || playerData.Players[0].Id != sender.id))
            {
                return false;
            }

            if (network.Mode == NetworkInterface.NetworkMode.Client
                && DeepWoodsMain.Instance.Game.World == null)
            {
                return false;
            }

            foreach (var player in playerData.Players)
            {
                DeepWoodsMain.Instance.Game.PlayerManager.UpdatePlayer(player);
            }
            return true;
        }

        private bool ProcessWelcomeMessage(PlayerId sender, WelcomeWorldData welcomeData)
        {
            if (welcomeData == null)
            {
                Debug.WriteLine($"ProcessWelcomeMessage: welcomeData == null!");
                return false;
            }

            DeepWoodsMain.Instance.Game.StartGame(welcomeData.SaveData);
            foreach (var player in welcomeData.Players)
            {
                if (player.Id == welcomeData.YourPlayerId)
                {
                    DeepWoodsMain.Instance.Game.PlayerManager.SpawnLocalPlayer(new(player.Id), new(player.X, player.Y));
                }
                else
                {
                    DeepWoodsMain.Instance.Game.PlayerManager.SpawnRemotePlayer(new(player.Id), new(player.X, player.Y));
                }
            }
            return true;
        }

        private bool SendMessageToAll(Message msg)
        {
            byte[] bytes = msg.ToBytes();
            return network.SendMessageToAll(bytes, bytes.Length, MessageMode.ReliableOrdered);
        }

        private bool SendMessage(PlayerId playerId, Message msg)
        {
            byte[] bytes = msg.ToBytes();
            return network.SendMessage(playerId, bytes, bytes.Length, MessageMode.ReliableOrdered);
        }

        private bool PlayerConnected(PlayerId playerId)
        {
            var welcomeData = new WelcomeWorldData()
            {
                SaveData = DeepWoodsMain.Instance.Game.SaveData,
                Players = DeepWoodsMain.Instance.Game.PlayerManager.Players.Select(p => new PlayerData()
                {
                    Id = p.ID.id,
                    X = p.position.X,
                    Y = p.position.Y
                }).ToList(),
                YourPlayerId = playerId.id
            };

            bool result = SendMessage(playerId, Message.Create(MessageType.Welcome, welcomeData));

            Debug.WriteLine($"PlayerConnected: {result}"); 
            return true;
        }

        private bool PlayerDisconnected(PlayerId playerId)
        {
            Debug.WriteLine($"PlayerDisconnected!");
            bool result = SendMessageToAll(Message.Create(MessageType.PlayerRemoved, new PlayerData()
            {
                Id = playerId.id
            }));
            // TODO: Make disconnected players sit down or "disappear"?
            //DeepWoodsMain.Instance.Game.PlayerManager.RemovePlayer(new(player.Id), new(player.X, player.Y));
            return true;
        }
    }
}
