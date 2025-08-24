
using Microsoft.Xna.Framework;
using System;

namespace DeepWoods.World.Generators
{
    internal abstract class Generator
    {
        protected readonly Tile[,] tiles;
        protected readonly int width;
        protected readonly int height;

        protected Generator(Tile[,] tiles)
        {
            this.tiles = tiles;
            width = tiles.GetLength(0);
            height = tiles.GetLength(1);
        }

        public abstract void Generate();

        protected bool IsInsideGrid(Point p)
        {
            return p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;
        }

    }
}
