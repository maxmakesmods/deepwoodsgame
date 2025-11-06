using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Main;
using DeepWoods.Players;
using System;
using System.Diagnostics;

namespace DeepWoods.Network
{
    public class NetworkManager
    {
        private readonly NetworkInterface network;

        private static readonly string Magic = "_DW_";                 // 4 bytes: _DW_
        private static readonly string GameVersion = "0.0.1-alpha";    // 3 bytes: 0.0.1

        private static readonly string InviteCode = "ABCDE";



        private NetworkManager(NetworkInterface network)
        {
            this.network = network;
            network.Init(ReceiveMessage, AcceptPlayer, PlayerConnected, PlayerDisconnected);
            bool result = network.StartHost();
            Debug.WriteLine($"NetworkManager.StartHost(): {result}");
        }

        private NetworkManager(NetworkInterface network, string host)
        {
            this.network = network;
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

        public void Update(float deltaTime)
        {
            network.Update(deltaTime);
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

            playerId = new PlayerId();

            Debug.WriteLine($"playerId: {playerId.id}");
            return true;
        }

        private bool ReceiveMessage(PlayerId sender, byte[] data, int dataOffset, int dataSize)
        {
            Debug.WriteLine($"ReceiveMessage!");
            SaveLoadHelper.SaveData saveData = data.FromBytes<SaveLoadHelper.SaveData>(dataOffset, dataSize);

            if (saveData != null)
            {
                Debug.WriteLine($"saveData != null!");
                DeepWoodsMain.Instance.Game.StartGame(saveData);
                return true;
            }
            else
            {
                Debug.WriteLine($"saveData == null!");
                return false;
            }
        }

        private bool PlayerConnected(PlayerId playerId)
        {
            byte[] saveData = DeepWoodsMain.Instance.Game.SaveData.ToBytes();
            bool result = network.SendMessage(playerId, saveData, saveData.Length, MessageMode.ReliableOrdered);
            Debug.WriteLine($"PlayerConnected: {result}"); 
            return true;
        }

        private bool PlayerDisconnected(PlayerId playerId)
        {
            Debug.WriteLine($"PlayerDisconnected!");
            return true;
        }
    }
}
