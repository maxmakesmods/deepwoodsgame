using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DeepWoods.World.Generators
{
    internal class UndergroundGenerator : ForestGenerator
    {
        private readonly VoidBiome voidBiome = new();

        protected override double GoalRatio => 0.3;
        protected override int BorderSize => 1;

        public UndergroundGenerator(Tile[,] tiles, List<IBiome> biomes, int seed)
            : base(tiles, biomes, seed)
        {
        }

        private void MakeFuzzyVoidBorder()
        {
            for (int x = 0; x < width; x++)
            {
                int maxy1 = rng.Next(Math.Max(4, height / 20));
                int maxy2 = rng.Next(Math.Max(4, height / 20));
                for (int y = 0; y <= maxy1; y++)
                {
                    tiles[x, y].biome = voidBiome;
                }
                for (int y = 0; y <= maxy2; y++)
                {
                    tiles[x, height - 1 - y].biome = voidBiome;
                }
            }
            for (int y = 0; y < width; y++)
            {
                int maxx1 = rng.Next(Math.Max(4, width / 20));
                int maxx2 = rng.Next(Math.Max(4, width / 20));
                for (int x = 0; x <= maxx1; x++)
                {
                    tiles[x, y].biome = voidBiome;
                }
                for (int x = 0; x <= maxx2; x++)
                {
                    tiles[width - 1 - x, y].biome = voidBiome;
                }
            }
        }

        private bool IsOpen(Point p)
        {
            if (!IsInsideGrid(p))
            {
                return false;
            }
            return tiles[p.X, p.Y].isOpen;
        }

        private bool IsOpenOrHasAtLeastOneOpenNeighbour(Point p)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (IsOpen(p + new Point(x, y)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Generate()
        {
            MakeFuzzyVoidBorder();
            base.Generate();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!IsOpenOrHasAtLeastOneOpenNeighbour(new(x, y)))
                    {
                        tiles[x, y].biome = voidBiome;
                    }
                }
            }
        }
    }
}
