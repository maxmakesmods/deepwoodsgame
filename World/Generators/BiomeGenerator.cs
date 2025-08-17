
using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace DeepWoods.World.Generators
{
    internal class BiomeGenerator : Generator
    {
        private readonly List<IBiome> biomes;

        public BiomeGenerator(Tile[,] tiles, List<IBiome> biomes)
            : base(tiles)
        {
            this.biomes = biomes;
        }

        public override void Generate()
        {
            VoidBiome voidBiome = new();

            int repeats = 3;

            int xcenter = width / 2;
            int ycenter = height / 2;

            int xradius = width / 6;
            int yradius = height / 6;
            float deltaAngle = 360;// 360f / (biomes.Count * repeats);

            int spiralWidth = Math.Max(2, Math.Min(width / 64, height / 64));

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
                        float i = (angle.Degrees + 360 * j) / deltaAngle;

                        float spiralx = xradius * i * MathF.Cos(angle);
                        float spiraly = yradius * i * MathF.Sin(angle);

                        float ourDistToMapCenter = new Vector2(relx, rely).Length();
                        float spiralDistToMapCenter = new Vector2(spiralx, spiraly).Length();

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

                        if (Math.Abs(ourDistToMapCenter - spiralDistToMapCenter) < spiralWidth)
                        {
                            tiles[x, y].biome = voidBiome;
                            tiles[x, y].isOpen = false;
                            break;
                        }
                        if (j == repeats - 1 && (ourDistToMapCenter - spiralDistToMapCenter) > spiralWidth)
                        {
                            tiles[x, y].biome = voidBiome;
                            tiles[x, y].isOpen = false;
                            break;
                        }
                    }
                }
            }

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
