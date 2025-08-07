using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class TemporaryTestBiome2 : IBiome
    {
        public GroundType OpenGroundType => GroundType.Snow;
        public GroundType ClosedGroundType => GroundType.Snow;
        public bool CanSpawnInThisBiome => false;

        public List<string> Trees => [
            "tree4"
        ];

        public List<string> Buildings => [
            "campfire",
            "portal_wood",
            "tower",
            "well"
        ];

        public List<string> Critters => [
        ];

        public List<string> Stuff => [
            "ring",
            "wizardhat",
            "helmet",
            "pendant",
            "pileofbooks",
            "crystal_ball",
        ];

        public float StuffDensity => 0.2f;
        public float BuildingDensity => 0.1f;
    }
}
