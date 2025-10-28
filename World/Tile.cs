
using DeepWoods.World.Biomes;

namespace DeepWoods.World
{
    public struct Tile
    {
        public IBiome biome;
        public GroundType groundType;
        public int x;
        public int y;
        public bool isOpen;
    }
}
