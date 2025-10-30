
using LiteNetLib;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DeepWoods.Network
{
    internal class LANNetwork : INetworkInterface, INetEventListener
    {
        private NetManager manager;


        public bool IsConnected => manager?.IsRunning ?? false;

        public int Ping { get; private set; } = 0;


        private readonly HashSet<INetworkInterface.ReceiveMessage> receivers = new();


        public bool Connect()
        {
            manager = new NetManager(this);
            return manager.Start(10000);
        }

        public bool Disconnect()
        {
            manager?.Stop();
            manager = null;
            return true;
        }

        public bool RegisterReceiveMessage(INetworkInterface.ReceiveMessage receiver)
        {
            return receivers.Add(receiver);
        }

        public bool SendMessage(int recipient, byte[] data)
        {
            return false;
        }

        public bool UnregisterReceiveMessage(INetworkInterface.ReceiveMessage receiver)
        {
            return receivers.Remove(receiver);
        }


        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.Accept();
        }

        public void OnPeerConnected(NetPeer peer)
        {

        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {

        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {

        }
    }
}
