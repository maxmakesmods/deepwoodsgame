using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class VoidBiome : IBiome
    {
        public float StuffDensity => 0;

        public float BuildingDensity => 0;

        public List<string> Trees => [];

        public List<string> Buildings => [];

        public List<string> Critters => [];

        public List<string> Stuff => [];

        public GroundType OpenGroundType => GroundType.Void;

        public GroundType ClosedGroundType => GroundType.Void;

        public bool CanSpawnInThisBiome => false;
    }
}
