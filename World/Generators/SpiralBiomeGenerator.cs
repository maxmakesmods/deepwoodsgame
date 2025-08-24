
using DeepWoods.Helpers;
using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace DeepWoods.World.Generators
{
    internal class SpiralBiomeGenerator : Generator
    {
        private static readonly int repeats = 3;
        private static readonly VoidBiome voidBiome = new();

        private readonly List<IBiome> biomes;

        private readonly int xcenter;
        private readonly int ycenter;
        private readonly int xradius;
        private readonly int yradius;
        private readonly int spiralRadius;

        public SpiralBiomeGenerator(Tile[,] tiles, List<IBiome> biomes)
            : base(tiles)
        {
            this.biomes = biomes;
            xcenter = width / 2;
            ycenter = height / 2;
            xradius = width / 6;
            yradius = height / 6;
            spiralRadius = Math.Max(2, Math.Min(width / 64, height / 64));
        }

        public override void Generate()
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int relx = x - xcenter;
                    int rely = y - ycenter;

                    var angle = Angle.FromVector(new(relx, rely));
                    angle.WrapPositive();

                    tiles[x, y].biome = null;

                    for (int j = 0; j < repeats; j++)
                    {
                        var spiralpos = GetSpiralPos(new(angle.Degrees + 360f * j, AngleType.Degree));
                        float spiralDistToMapCenter = spiralpos.Length();
                        float ourDistToMapCenter = new Vector2(relx, rely).Length();

                        if (tiles[x, y].biome == null &&
                            ourDistToMapCenter < spiralDistToMapCenter)
                        {
                            int biomeIndex;

                            if (j == 0)
                            {
                                biomeIndex = 0;
                            }
                            else if (j == 1)
                            {
                                int anglesPerBiome = 150;
                                biomeIndex = (int)(angle.Degrees / anglesPerBiome);
                            }
                            else if (j == 2)
                            {
                                int anglesPerBiome = 70;
                                biomeIndex = (int)(2 + angle.Degrees / anglesPerBiome);
                                if (biomeIndex > 6)
                                {
                                    Debug.WriteLine($"angle.Degrees: {angle.Degrees}");
                                }
                            }
                            else
                            {
                                biomeIndex = int.MaxValue;
                            }


                            if (biomeIndex < biomes.Count)
                            {
                                tiles[x, y].biome = biomes[biomeIndex];
                            }
                            else
                            {
                                tiles[x, y].biome = voidBiome;
                            }
                        }

                        if (Math.Abs(ourDistToMapCenter - spiralDistToMapCenter) < spiralRadius)
                        {
                            tiles[x, y].biome = voidBiome;
                            tiles[x, y].isOpen = false;
                            break;
                        }
                        if (j == repeats - 1 && (ourDistToMapCenter - spiralDistToMapCenter) > spiralRadius)
                        {
                            tiles[x, y].biome = voidBiome;
                            tiles[x, y].isOpen = false;
                            break;
                        }
                    }
                }
            }

            AddCirclyEnd(biomes[biomes.Count - 1]);

            FillVoid(voidBiome);
        }

        private Vector2 GetSpiralPos(Angle angle)
        {
            float i = angle.Degrees / 360f;
            float spiralX = xradius * i * MathF.Cos(angle);
            float spiralY = yradius * i * MathF.Sin(angle);
            return new(spiralX, spiralY);
        }

        private void AddCirclyEnd(IBiome lastBiome)
        {
            Angle innerAngle = new(350 + 360f * 1, AngleType.Degree);
            Vector2 innerSpiralPos = GetSpiralPos(innerAngle);
            Angle outerAngle = new(350 + 360f * 2, AngleType.Degree);
            Vector2 outerSpiralPos = GetSpiralPos(outerAngle);

            int radius = (int)MathF.Ceiling((innerSpiralPos - outerSpiralPos).Length() / 2 - spiralRadius);
            Point midPoint = ((innerSpiralPos + outerSpiralPos) / 2).RoundToPoint();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (new Vector2(x, y).Length() < radius)
                    {
                        int px = xcenter + midPoint.X + x;
                        int py = ycenter + midPoint.Y + y;
                        if (IsInsideGrid(new Point(px, py)))
                        {
                            tiles[px, py].biome = lastBiome;
                        }
                    }
                }
            }
        }

        private void FillVoid(VoidBiome voidBiome)
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
                    }
                }
            }
        }
    }
}
