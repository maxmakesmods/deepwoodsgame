using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepWoods.World.Generators
{
    internal class LabyrinthGenerator : Generator
    {
        private static readonly Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

        protected override void GenerateImpl()
        {
            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(1, 1));
            tiles[1, 1].isOpen = true;

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
        }
    }
}
