
namespace DeepWoods.Network
{
    internal interface INetworkInterface
    {
        delegate bool ReceiveMessage(int sender, byte[] data);

        bool Connect();
        bool Disconnect();
        bool SendMessage(int recipient, byte[] data);
        bool RegisterReceiveMessage(ReceiveMessage receiver);
        bool UnregisterReceiveMessage(ReceiveMessage receiver);

        bool IsConnected { get; }
        int Ping { get; }
    }
}
