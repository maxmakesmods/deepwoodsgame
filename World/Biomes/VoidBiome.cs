using DeepWoods.Objects;
using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class VoidBiome : IBiome
    {
        public float StuffDensity => 0;

        public float BuildingDensity => 0;

        public List<string> Trees => [];

        public List<string> Buildings => [];

        public List<CritterDefinitions.Critter> Critters => [];

        public List<string> Stuff => [];

        public string CaveObjectId { get; set; } = null;

        public GroundType OpenGroundType => GroundType.Void;

        public GroundType ClosedGroundType => GroundType.Void;

        public bool CanSpawnInThisBiome => false;

        public bool IsVoid => true;
    }
}
