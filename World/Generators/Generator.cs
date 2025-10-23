
using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DeepWoods.World.Generators
{
    internal abstract class Generator
    {
        protected Random rng;
        protected List<IBiome> biomes;
        protected Tile[,] tiles;
        protected int width;
        protected int height;

        public void Generate(Tile[,] tiles, List<IBiome> biomes, int seed)
        {
            this.biomes = biomes;
            this.tiles = tiles;
            width = tiles.GetLength(0);
            height = tiles.GetLength(1);
            rng = new Random(seed);
            GenerateImpl();
        }

        protected abstract void GenerateImpl();

        protected bool IsInsideGrid(Point p)
        {
            return p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;
        }

    }
}
