
using DeepWoods.World.Biomes;
using System;

namespace DeepWoods.World.Generators
{
    internal class SingleBiomeGenerator : Generator
    {
        protected override void GenerateImpl()
        {
            if (biomes.Count != 1)
            {
                throw new ArgumentException("must be 1 biome!");
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y].biome = biomes[0];
                }
            }
        }
    }
}
