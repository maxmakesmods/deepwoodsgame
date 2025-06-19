using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DeepWoods.World.Generators
{
    internal class LabyrinthGenerator : Generator
    {
        private readonly Tile[,] tiles;
        private readonly int width;
        private readonly int height;
        private readonly Random rng;

        private static readonly Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

        public LabyrinthGenerator(int width, int height, int seed)
        {
            tiles = new Tile[width, height];
            this.width = width;
            this.height = height;
            rng = new Random(seed);
        }

        private bool IsInsideGrid(Point next)
        {
            return next.X >= 0 && next.X < width && next.Y >= 0 && next.Y < height;
        }

        public override Tile[,] Generate()
        {
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(1, 1));

            while (stack.Count > 0)
            {
                Point p = stack.Peek();
                bool foundAnyPath = false;

                var randomizedDirections = directions.OrderBy(_ => rng.Next()).ToList();
                foreach (var direction in randomizedDirections)
                {
                    Point next = p + direction;
                    Point nextnext = p + direction + direction;

                    if (!IsInsideGrid(nextnext))
                    {
                        continue;
                    }

                    if (tiles[nextnext.X, nextnext.Y].isOpen)
                    {
                        continue;
                    }

                    tiles[next.X, next.Y].isOpen = true;
                    tiles[nextnext.X, nextnext.Y].isOpen = true;
                    stack.Push(nextnext);
                    foundAnyPath = true;
                    break;
                }

                if (!foundAnyPath)
                {
                    stack.Pop();
                }
            }

            return tiles;
        }
    }
}
