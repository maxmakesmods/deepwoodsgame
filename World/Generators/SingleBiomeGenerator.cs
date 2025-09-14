
using DeepWoods.World.Biomes;

namespace DeepWoods.World.Generators
{
    internal class SingleBiomeGenerator : Generator
    {
        private readonly IBiome biome;

        public SingleBiomeGenerator(Tile[,] tiles, IBiome biome)
            : base(tiles)
        {
            this.biome = biome;
        }

        public override void Generate()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y].biome = biome;
                }
            }
        }
    }
}
