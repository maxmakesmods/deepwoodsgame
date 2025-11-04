
using DeepWoods.Players;

namespace DeepWoods.Network
{
    public abstract class NetworkInterface
    {
        public delegate bool AcceptPlayer(byte[] data, int dataSize, out PlayerId playerId);
        public delegate bool ReceiveMessage(PlayerId sender, byte[] data, int dataSize);
        public delegate bool PlayerDisconnected(PlayerId playerId);

        protected ReceiveMessage receiver;
        protected AcceptPlayer accepter;
        protected PlayerDisconnected disconnectedHandler;

        public virtual void Init(ReceiveMessage receiver, AcceptPlayer accepter, PlayerDisconnected disconnectedHandler)
        {
            this.receiver = receiver;
            this.accepter = accepter;
            this.disconnectedHandler = disconnectedHandler;
        }

        public abstract bool StartHost();
        public abstract bool StartClient(string host, byte[] data, int dataSize);
        public abstract bool Disconnect();
        public abstract bool SendMessage(PlayerId recipient, byte[] data, int dataSize, MessageMode mode);
        public abstract bool SendMessageToAll(byte[] data, int dataSize, MessageMode mode);
        public abstract void SwitchPlayers(PlayerId id1, PlayerId id2);
        public abstract void Update(float deltaTime);

        public abstract bool IsConnected { get; }
    }
}
