using DeepWoods.Game;
using DeepWoods.Players;
using System;

namespace DeepWoods.Network
{
    public class NetworkManager
    {
        private readonly DeepWoodsGame game;
        private readonly NetworkInterface network;

        struct ConnectionPayload
        {
            byte[] magic;       // 4 bytes: _DW_
            byte[] gameVersion; // 3 bytes: 1.0.0
            string inviteCode;  // 5 bytes: ABCDE
            long playerId;      // 8 bytes: uuid
        }



        public NetworkManager(DeepWoodsGame game, NetworkInterface network)
        {
            this.game = game;
            this.network = network;
            network.Init(ReceiveMessage, AcceptPlayer, PlayerDisconnected);
            network.StartHost();
        }

        public NetworkManager(DeepWoodsGame game, NetworkInterface network, string host)
        {
            this.game = game;
            this.network = network;
            network.Init(ReceiveMessage, AcceptPlayer, PlayerDisconnected);
            network.StartClient(host);
        }


        private bool AcceptPlayer(byte[] data, int dataSize, out PlayerId playerId)
        {
            playerId = new PlayerId();
            return true;
        }

        private bool ReceiveMessage(PlayerId sender, byte[] data, int dataSize)
        {
            return true;
        }

        private bool PlayerDisconnected(PlayerId playerId)
        {
            return true;
        }
    }
}
