
using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace DeepWoods.World.Generators
{
    internal class EightFigureBiomeGenerator : Generator
    {
        private static readonly VoidBiome voidBiome = new();

        private int xcenter1;
        private int ycenter1;
        private int xcenter2;
        private int ycenter2;
        private int xradius;
        private int yradius;
        private int circleRadius;

        protected override void GenerateImpl()
        {
            xradius = width / 6;
            yradius = height / 6;

            circleRadius = Math.Max(8, Math.Min(xradius / 2, yradius / 2));

            xcenter1 = width / 2 - xradius;
            ycenter1 = height / 2;

            xcenter2 = width / 2 + xradius;
            ycenter2 = height / 2;

            FillNull();

            var startBiome = biomes[0];

            int biomesPerCircle = (biomes.Count - 1) / 2;

            List<IBiome> firstCircleBiomes = biomes[1..(1 + biomesPerCircle)];
            List<IBiome> secondCircleBiomes = biomes[(1 + biomesPerCircle)..];

            int xstart1 = 1;
            int xend1 = width / 2;
            int xstart2 = width / 2;
            int xend2 = width - 1;

            GenerateCircle(firstCircleBiomes, xstart1, xend1, xcenter1, ycenter1, false);
            GenerateCircle(secondCircleBiomes, xstart2, xend2, xcenter2, ycenter2, true);

            //AddCirclyEnd(startBiome);

            FillVoid();
        }

        private void AddCirclyEnd(IBiome biome)
        {
            int radius = (int)MathF.Min(xradius, yradius) / 2;
            Point midPoint = new(width / 2, height / 2);
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (new Vector2(x, y).Length() <= radius)
                    {
                        int px = midPoint.X + x;
                        int py = midPoint.Y + y;
                        if (IsInsideGrid(new Point(px, py)))
                        {
                            tiles[px, py].biome = biome;
                        }
                    }
                }
            }
        }

        private void GenerateCircle(List<IBiome> biomes, int xstart, int xend, int xcenter, int ycenter, bool flipped)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int relx = x - xcenter;
                    int rely = y - ycenter;

                    var angle = Angle.FromVector(new(flipped ? -relx : relx, rely));
                    angle.WrapPositive();

                    var circlepos = GetCirclePos(angle);
                    float circleDistToMapCenter = circlepos.Length();
                    float circleDistInner = circleDistToMapCenter - circleRadius;
                    float circleDistOuter = circleDistToMapCenter + circleRadius;
                    float ourDistToMapCenter = new Vector2(relx, rely).Length();

                    if (tiles[x, y].biome != biomes[0] && ourDistToMapCenter >= circleDistInner && ourDistToMapCenter <= circleDistOuter)
                    {
                        if (x < xstart || x >= xend)
                        {
                            tiles[x, y].biome = biomes[0];
                        }
                        else
                        {
                            float anglesPerBiome = 360f / biomes.Count;
                            int biomeIndex = (int)Math.Clamp(angle.Degrees / anglesPerBiome, 0, biomes.Count - 1);
                            tiles[x, y].biome = biomes[biomeIndex];
                        }
                    }
                }
            }
        }

        private Vector2 GetCirclePos(Angle angle)
        {
            float spiralX = xradius * MathF.Cos(angle);
            float spiralY = yradius * MathF.Sin(angle);
            return new(spiralX, spiralY);
        }

        private void FillNull()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y].biome = null;
                }
            }
        }

        private void FillVoid()
        {
            for (int x = 0; x < width; x++)
            {
                tiles[x, 0].biome = voidBiome;
                tiles[x, 0].isOpen = false;
                tiles[x, height - 1].biome = voidBiome;
                tiles[x, height - 1].isOpen = false;
            }

            for (int y = 0; y < height; y++)
            {
                tiles[0, y].biome = voidBiome;
                tiles[0, y].isOpen = false;
                tiles[width - 1, y].biome = voidBiome;
                tiles[width - 1, y].isOpen = false;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y].biome == null)
                    {
                        tiles[x, y].biome = voidBiome;
                        tiles[x, y].isOpen = false;
                    }
                }
            }
        }
    }
}
