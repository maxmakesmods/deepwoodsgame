using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepWoods.World.Generators
{
    internal class ForestGenerator : Generator
    {
        private readonly Random rng;

        private static readonly Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

        class Region
        {
            public HashSet<Point> tiles = new();
            public Point anchor;
        }

        public ForestGenerator(Tile[,] tiles, int seed)
            : base(tiles)
        {
            rng = new Random(seed);
        }

        private bool IsInsideGrid(Point p)
        {
            return p.X >= 0 && p.X < width
                && p.Y >= 0 && p.Y < height
                && !tiles[p.X, p.Y].biome.IsVoid;
        }

        private bool CanGenerateHere(Point p)
        {
            if (!IsInsideGrid(p))
                return false;

            int borderSize = 2;
            for (int i = 1; i <= borderSize; i++)
            {
                if (!IsInsideGrid(p + new Point(0, i))
                    || !IsInsideGrid(p + new Point(0, -i))
                    || !IsInsideGrid(p + new Point(i, 0))
                    || !IsInsideGrid(p + new Point(-i, 0)))
                {
                    return false;
                }
            }

            return true;
        }

        private double CurrentRatio()
        {
            int totalTiles = 0;
            int openTiles = 0;
            foreach (var tile in tiles)
            {
                if (tile.biome.IsVoid)
                    continue;

                if (tile.isOpen)
                    openTiles++;
                totalTiles++;
            }
            return openTiles / (double)totalTiles;
        }

        public override void Generate()
        {
            int numSteps = Math.Max(10, width * height / 100);
            double goalRatio = 0.5;
            while (CurrentRatio() < goalRatio)
            {
                GenerateOpenPatch(new(rng.Next(width), rng.Next(height)), numSteps);
            }

            int treeBorderSize = 3;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < treeBorderSize; y++)
                {
                    tiles[x, y].isOpen = false;
                    tiles[x, height - 1 - y].isOpen = false;
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < treeBorderSize; x++)
                {
                    tiles[x, y].isOpen = false;
                    tiles[width - 1 - x, y].isOpen = false;
                }
            }

            ConnectRegions();
        }

        private List<Region> CollectRegions()
        {
            List<Region> regions = new();
            bool[,] visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y].isOpen && !visited[x, y])
                    {
                        Region region = new Region();
                        Queue<Point> queue = new Queue<Point>();
                        queue.Enqueue(new Point(x, y));
                        visited[x, y] = true;

                        while (queue.Count > 0)
                        {
                            Point p = queue.Dequeue();
                            region.tiles.Add(p);
                            foreach (var direction in directions)
                            {
                                Point neighbour = p + direction;
                                if (CanGenerateHere(neighbour) && tiles[neighbour.X, neighbour.Y].isOpen && !visited[neighbour.X, neighbour.Y])
                                {
                                    queue.Enqueue(neighbour);
                                    visited[neighbour.X, neighbour.Y] = true;
                                }
                            }
                        }

                        region.anchor = region.tiles.ToList()[rng.Next(region.tiles.Count)];
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        private void ConnectRegions()
        {
            var regions = CollectRegions();
            while (regions.Count > 1)
            {
                Region regionA = regions[rng.Next(regions.Count)];
                Region regionB = regions[rng.Next(regions.Count)];
                if (TryConnectTwoRegions(regionA, regionB))
                {
                    regions = CollectRegions();
                }
            }
        }

        private bool TryConnectTwoRegions(Region regionA, Region regionB)
        {
            if (regionA == regionB)
            {
                return false;
            }

            Point anchorA = regionA.tiles.ToList()[rng.Next(regionA.tiles.Count)];
            Point anchorB = regionB.tiles.ToList()[rng.Next(regionB.tiles.Count)];

            List<Point> pointsToOpen = [];

            Point currentPoint = anchorA;
            while (currentPoint != anchorB)
            {
                int distX = Math.Abs(anchorB.X - currentPoint.X);
                int distY = Math.Abs(anchorB.Y - currentPoint.Y);

                if (distY == 0 || rng.NextSingle() * distX > rng.NextSingle() * distY)
                {
                    currentPoint.X += Math.Sign(anchorB.X - currentPoint.X);
                }
                else
                {
                    currentPoint.Y += Math.Sign(anchorB.Y - currentPoint.Y);
                }

                if (!CanGenerateHere(currentPoint))
                {
                    return false;
                }

                pointsToOpen.Add(currentPoint);
            }

            foreach (Point point in pointsToOpen)
            {
                tiles[point.X, point.Y].isOpen = true;
            }

            return true;
        }

        private void GenerateOpenPatch(Point p, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                if (CanGenerateHere(p))
                {
                    tiles[p.X, p.Y].isOpen = true;
                }
                p += directions[rng.Next(directions.Length)];
            }
        }
    }
}
