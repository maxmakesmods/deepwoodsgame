
namespace DeepWoods.Network
{
    public enum MessageMode
    {
        UnreliableUnordered = 0,
        UnreliableSequenced = 1,
        ReliableOrdered = 2,
        ReliableUnordered = 3,
        ReliableSequenced = 4
    }
}
