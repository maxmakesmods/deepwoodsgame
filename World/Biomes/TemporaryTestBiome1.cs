using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class TemporaryTestBiome1 : IBiome
    {
        public GroundType OpenGroundType => GroundType.Mud;
        public GroundType ClosedGroundType => GroundType.Mud;
        public bool CanSpawnInThisBiome => false;

        public List<string> Trees => [
            "tree3"
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
            "bluecrystal",
            "greencrystal",
            "yellowcrystal",
            "pinkcrystal",
            "purplecrystal",
            "redcrystal",
            "whitecrystal",
        ];

        public float StuffDensity => 0.2f;
        public float BuildingDensity => 0.1f;
        public bool IsVoid => false;
    }
}
