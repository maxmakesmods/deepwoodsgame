
using DeepWoods.Helpers;
using System;
using System.Collections.Generic;

namespace DeepWoods.Network.Data
{
    public class WelcomeWorldData
    {
        public SaveLoadHelper.SaveData SaveData { get; set; }
        public List<PlayerData> Players { get; set; }
        public Guid YourPlayerId { get; set; }
    }
}
