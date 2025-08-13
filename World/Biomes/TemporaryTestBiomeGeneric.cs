using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class TemporaryTestBiomeGeneric : IBiome
    {
        private readonly GroundType groundType;

        public GroundType OpenGroundType => groundType;
        public GroundType ClosedGroundType => groundType;
        public bool CanSpawnInThisBiome => false;

        public TemporaryTestBiomeGeneric(GroundType groundType)
        {
            this.groundType = groundType;
        }

        public List<string> Trees => [
            "tree4"
        ];

        public List<string> Buildings => [
        ];

        public List<string> Critters => [
        ];

        public List<string> Stuff => [
        ];

        public float StuffDensity => 0.2f;
        public float BuildingDensity => 0.1f;
    }
}
