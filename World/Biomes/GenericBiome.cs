using DeepWoods.Objects;
using System.Collections.Generic;

namespace DeepWoods.World.Biomes
{
    internal class GenericBiome : IBiome
    {
        public GroundType OpenGroundType { get; private set; }
        public GroundType ClosedGroundType { get; private set; }
        public bool CanSpawnInThisBiome { get; private set; }
        public bool IsVoid { get; set; } = false;

        public List<string> Trees { get; set; } = [];
        public List<string> Buildings { get; set; } = [];
        public List<CritterDefinitions.Critter> Critters { get; set; } = [];
        public List<string> Stuff { get; set; } = [];
        public string CaveObjectId { get; set; } = "crystal ball";

        public float StuffDensity { get; set; } = 0.2f;
        public float BuildingDensity { get; set; } = 0.1f;

        public GenericBiome(GroundType groundType)
        {
            OpenGroundType = groundType;
            ClosedGroundType = groundType;
        }

        public GenericBiome(GroundType openGroundType, GroundType closedGroundType, bool canSpawn = true)
        {
            OpenGroundType = openGroundType;
            ClosedGroundType = closedGroundType;
            CanSpawnInThisBiome = canSpawn;
        }
    }
}
