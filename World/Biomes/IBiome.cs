using DeepWoods.Objects;
using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal interface IBiome
    {
        public float StuffDensity { get; }
        public float BuildingDensity { get; }
        public List<string> Trees { get; }
        public List<string> Buildings { get; }
        public List<string> Stuff { get; }
        public List<CritterDefinitions.Critter> Critters { get; }
        public GroundType OpenGroundType { get; }
        public GroundType ClosedGroundType { get; }
        public bool CanSpawnInThisBiome { get; }
        bool IsVoid { get; }
    }
}
