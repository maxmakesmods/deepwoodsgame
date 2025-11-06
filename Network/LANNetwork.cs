
using DeepWoods.Players;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DeepWoods.Network
{
    internal class LANNetwork : NetworkInterface, INetEventListener
    {
        private NetManager manager;


        public override bool IsConnected => manager?.IsRunning ?? false;


        private HashSet<NetPeer> peers = new();

        private Dictionary<NetPeer, PlayerId> peerToPlayer = new();

        private Dictionary<PlayerId, NetPeer> playerToPeer = new();

        public override bool StartHost()
        {
            Disconnect();
            manager = new NetManager(this);
            return manager.Start(10000);
        }

        public override bool StartClient(string host, byte[] data, int dataSize)
        {
            Disconnect();
            manager = new NetManager(this);
            manager.Start();
            var peer = manager.Connect(host, 10000, NetDataWriter.FromBytes(data, 0, dataSize));
            return peer != null;
        }

        public override bool Disconnect()
        {
            manager?.Stop();
            manager = null;
            peers.Clear();
            peerToPlayer.Clear();
            playerToPeer.Clear();
            return true;
        }

        public override void Update(float deltaTime)
        {
            manager?.PollEvents();
        }

        public override void SwitchPlayers(PlayerId id1, PlayerId id2)
        {
            bool hasPeer1 = playerToPeer.TryGetValue(id1, out NetPeer peer1);
            bool hasPeer2 = playerToPeer.TryGetValue(id2, out NetPeer peer2);

            playerToPeer.Remove(id1);
            playerToPeer.Remove(id2);

            if (hasPeer1)
            {
                playerToPeer[id2] = peer1;
                peerToPlayer[peer1] = id2;
            }

            if (hasPeer2)
            {
                playerToPeer[id1] = peer2;
                peerToPlayer[peer2] = id1;
            }
        }

        public override bool SendMessage(PlayerId recipient, byte[] data, int dataSize, MessageMode mode)
        {
            if (IsConnected)
            {
                if (playerToPeer.TryGetValue(recipient, out NetPeer peer))
                {
                    peer.Send(data, 0, dataSize, ModeToMethod(mode));
                }
                return true;
            }
            return false;
        }

        public override bool SendMessageToAll(byte[] data, int dataSize, MessageMode mode)
        {
            if (IsConnected)
            {
                manager.SendToAll(data, 0, dataSize, ModeToMethod(mode));
                return true;
            }
            return false;
        }

        private static DeliveryMethod ModeToMethod(MessageMode mode)
        {
            return mode switch
            {
                MessageMode.UnreliableUnordered => DeliveryMethod.Unreliable,
                MessageMode.UnreliableSequenced => DeliveryMethod.Sequenced,
                MessageMode.ReliableOrdered => DeliveryMethod.ReliableOrdered,
                MessageMode.ReliableUnordered => DeliveryMethod.ReliableUnordered,
                MessageMode.ReliableSequenced => DeliveryMethod.ReliableSequenced,
                _ => throw new ArgumentException("unknown mode", nameof(mode)),
            };
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            Debug.WriteLine("OnConnectionRequest");
            if (accepter.Invoke(request.Data.RawData, request.Data.UserDataOffset, request.Data.UserDataSize, out PlayerId playerId))
            {
                var peer = request.Accept();
                playerToPeer.Add(playerId, peer);
                peerToPlayer.Add(peer, playerId);
                peers.Add(peer);
            }
            else
            {
                request.Reject(); // TODO: RejectForce();??
            }
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            Debug.WriteLine("OnNetworkReceive");
            if (peerToPlayer.TryGetValue(peer, out PlayerId playerId))
            {
                receiver.Invoke(playerId, reader.RawData, reader.UserDataOffset, reader.UserDataSize);
            }
            else
            {
                // TODO Handle unknown peer error
            }
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.WriteLine("OnPeerDisconnected");
            if (peerToPlayer.TryGetValue(peer, out PlayerId playerId))
            {
                disconnectedHandler.Invoke(playerId);
                playerToPeer.Remove(playerId);
            }
            peers.Remove(peer);
            peerToPlayer.Remove(peer);
        }


        public void OnPeerConnected(NetPeer peer)
        {
            Debug.WriteLine("OnPeerConnected");
            if (peerToPlayer.TryGetValue(peer, out PlayerId playerId))
            {
                connectedHandler.Invoke(playerId);
            }
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            Debug.WriteLine("OnNetworkLatencyUpdate");
            // TODO: Do we need this?
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Debug.WriteLine("OnNetworkError");
            // TODO: Do we need this?
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Debug.WriteLine("OnNetworkReceiveUnconnected");
            // TODO: Do we need this?
        }

    }
}
