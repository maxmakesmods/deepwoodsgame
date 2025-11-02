
using DeepWoods.Players;

namespace DeepWoods.Network
{
    internal interface INetworkInterface
    {
        delegate bool AcceptPlayer(byte[] data, int dataSize, out PlayerId playerId);
        delegate bool ReceiveMessage(PlayerId sender, byte[] data, int dataSize);
        delegate bool PlayerDisconnected(PlayerId playerId);

        bool StartHost();
        bool StartClient(string host, byte[] data, int dataSize);
        bool Disconnect();
        bool SendMessage(PlayerId recipient, byte[] data, int dataSize, MessageMode mode);
        bool SendMessageToAll(byte[] data, int dataSize, MessageMode mode);
        void SwitchPlayers(PlayerId id1, PlayerId id2);
        void Update(float deltaTime);

        bool IsConnected { get; }
        int Ping { get; }
    }
}
