using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class TemperateForestBiome : IBiome
    {
        public GroundType OpenGroundType => GroundType.Grass;
        public GroundType ClosedGroundType => GroundType.ForestFloor;
        public bool CanSpawnInThisBiome => true;

        public List<string> Trees => [
            "tree1",
            "tree2"
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
            "flower1",
            "flower2",
            "flower3",
            "flower4",
            "herb1",
            "herb2",
            "herb3",
            "herb4",
        ];

        public float StuffDensity => 0.2f;
        public float BuildingDensity => 0.1f;

        public bool IsVoid => false;
    }
}
