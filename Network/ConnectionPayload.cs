
namespace DeepWoods.Network
{
    public struct ConnectionPayload
    {
        // total: 28 bytes
        public string Magic { get; set; }        //  4 bytes: _DW_
        public string GameVersion { get; set; }  //  3 bytes: 0.0.1
        public string InviteCode { get; set; }   //  5 bytes: ABCDE
        public long PlayerId { get; set; }       // 16 bytes: uuid
    }
}
