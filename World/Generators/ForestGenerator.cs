using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepWoods.World.Generators
{
    internal class ForestGenerator : Generator
    {
        private static readonly Point[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

        protected virtual double GoalRatio => 0.5;
        protected virtual int BorderSize => 2;

        class Region
        {
            public HashSet<Point> tiles = new();
            public Point center;
        }

        private bool IsValidPoint(Point p)
        {
            return IsInsideGrid(p) && !tiles[p.X, p.Y].biome.IsVoid;
        }

        private bool CanGenerateHere(Point p)
        {
            if (!IsValidPoint(p))
                return false;

            for (int i = 1; i <= BorderSize; i++)
            {
                if (!IsValidPoint(p + new Point(0, i))
                    || !IsValidPoint(p + new Point(0, -i))
                    || !IsValidPoint(p + new Point(i, 0))
                    || !IsValidPoint(p + new Point(-i, 0)))
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

        protected override void GenerateImpl()
        {
            int numSteps = Math.Max(10, width * height / 100);
            while (CurrentRatio() < GoalRatio)
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

        private bool IsRegionTile(int x, int y, IBiome biome)
        {
            return CanGenerateHere(new(x, y)) && tiles[x, y].biome == biome && tiles[x, y].isOpen;
        }

        private List<Region> CollectRegions(IBiome biome)
        {
            List<Region> regions = new();
            bool[,] visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsRegionTile(x, y, biome) && !visited[x, y])
                    {
                        Region region = new Region();
                        Queue<Point> queue = new Queue<Point>();
                        queue.Enqueue(new Point(x, y));
                        visited[x, y] = true;

                        Point minPoint = new(int.MaxValue, int.MaxValue);
                        Point maxPoint = new(int.MinValue, int.MinValue);

                        while (queue.Count > 0)
                        {
                            Point p = queue.Dequeue();
                            region.tiles.Add(p);
                            minPoint.X = Math.Min(p.X, minPoint.X);
                            minPoint.Y = Math.Min(p.Y, minPoint.Y);
                            maxPoint.X = Math.Max(p.X, maxPoint.X);
                            maxPoint.Y = Math.Max(p.Y, maxPoint.Y);
                            foreach (var direction in directions)
                            {
                                Point neighbour = p + direction;
                                if (IsRegionTile(neighbour.X, neighbour.Y, biome) && !visited[neighbour.X, neighbour.Y])
                                {
                                    queue.Enqueue(neighbour);
                                    visited[neighbour.X, neighbour.Y] = true;
                                }
                            }
                        }

                        Point centerPoint = (minPoint + maxPoint) / new Point(2, 2);
                        Point mostCenterPoint = new(int.MaxValue, int.MaxValue);
                        float lastLengthSquared = float.MaxValue;

                        foreach (var tile in region.tiles)
                        {
                            float lengthSquared = (tile - centerPoint).ToVector2().LengthSquared();
                            if (lengthSquared < lastLengthSquared)
                            {
                                mostCenterPoint = tile;
                                lastLengthSquared = lengthSquared;
                            }
                        }

                        region.center = mostCenterPoint;
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        private void ConnectRegions()
        {
            bool debugbreaker = false;

            List<Region> biomeRegions = [];
            foreach (var biome in biomes)
            {
                var regions = CollectRegions(biome);
                int attemptCounter = 0;
                while (regions.Count > 1 && !debugbreaker)
                {
                    Region regionA = regions[rng.Next(regions.Count)];
                    //Region regionB = regions[rng.Next(regions.Count)];
                    regions.Sort((r1, r2) => (r1.center - regionA.center).ToVector2().LengthSquared().CompareTo((r2.center - regionA.center).ToVector2().LengthSquared()));
                    Region regionB = regions[1]; // 0 is itself
                    if (TryConnectTwoRegions(regionA, regionB, biome))
                    {
                        regions = CollectRegions(biome);
                        attemptCounter = 0;
                    }
                    else
                    {
                        attemptCounter++;
                        if (attemptCounter >= 100)
                        {
                            FillInSmallestRegion(regions);
                            regions = CollectRegions(biome);
                            attemptCounter = 0;
                        }
                    }
                }
                biomeRegions.Add(regions[0]);
            }

            for (int i = 0; i < biomeRegions.Count - 1; i++)
            {
                while (!TryConnectTwoRegions(biomeRegions[i], biomeRegions[i + 1], biomes[i + 1]));
            }
        }

        private void FillInSmallestRegion(List<Region> regions)
        {
            regions.Sort((r1, r2) => r1.tiles.Count.CompareTo(r2.tiles.Count));
            foreach (var p in regions[0].tiles)
            {
                tiles[p.X, p.Y].isOpen = false;
            }
        }

        private bool TryConnectTwoRegions(Region regionA, Region regionB, IBiome highestAllowedBiome)
        {
            if (regionA == regionB)
            {
                return false;
            }

            int numPaths = 3;

            for (int i = 0; i < numPaths; i++)
            {
                List<Point> pointsToOpen = [];

                int numAttempts = 100;
                for (int j = 0; j < numAttempts; j++)
                {
                    var attemptPointsToOpen = TryFindPathBetweenRegions(regionA, regionB, highestAllowedBiome);
                    if (attemptPointsToOpen == null)
                    {
                        continue;
                    }
                    if (pointsToOpen.Count == 0 || attemptPointsToOpen.Count < pointsToOpen.Count)
                    {
                        pointsToOpen = attemptPointsToOpen;
                    }
                }

                foreach (Point point in pointsToOpen)
                {
                    tiles[point.X, point.Y].isOpen = true;
                }
            }

            return true;
        }

        private List<Point> TryFindPathBetweenRegions(Region regionA, Region regionB, IBiome highestAllowedBiome)
        {
            List<Point> pointsToOpen = [];

            Point anchorA = regionA.tiles.ToList()[rng.Next(regionA.tiles.Count)];
            Point anchorB = regionB.tiles.ToList()[rng.Next(regionB.tiles.Count)];

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

                if (!CanGenerateHere(currentPoint)
                    || biomes.IndexOf(tiles[currentPoint.X, currentPoint.Y].biome) > biomes.IndexOf(highestAllowedBiome))
                {
                    return null;
                }

                pointsToOpen.Add(currentPoint);
            }

            return pointsToOpen;
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
