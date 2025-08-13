using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.World.Biomes;
using DeepWoods.World.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;

namespace DeepWoods.World
{
    internal class Terrain
    {
        public readonly static int CellSize = 32;
        private readonly static int DitherSize = 4;

        private readonly VertexPositionColorTexture[] drawingQuad;
        private readonly short[] drawingIndices = [0, 1, 2, 0, 2, 3];
        private readonly Tile[,] tiles;
        private readonly Texture2D terrainGridTexture;
        private readonly int seed;
        private readonly int width;
        private readonly int height;
        private readonly int blueNoiseDitherChannel;
        private readonly Vector2 blueNoiseDitherOffset;
        private readonly int blueNoiseVariantChannel;
        private readonly Vector2 blueNoiseVariantOffset;
        private readonly int blueNoiseSineXChannel;
        private readonly Vector2 blueNoiseSineXOffset;
        private readonly int blueNoiseSineYChannel;
        private readonly Vector2 blueNoiseSineYOffset;
        private readonly Random rng;

        public int Seed => seed;
        public int Width => width;
        public int Height => height;

        private class PatchCenter
        {
            public int x;
            public int y;
            public int radiusSqrd;
            public IBiome biome;
        }

        public Terrain(AllTheThings att, int seed, int width, int height)
        {
            rng = new Random(seed);
            this.seed = seed;
            this.width = width;
            this.height = height;


            List<IBiome> biomes = [
                new TemperateForestBiome(),
                new TemporaryTestBiome1(),
                new TemporaryTestBiome2(),
                new TemporaryTestBiomeGeneric(GroundType.Sand),
                new TemporaryTestBiomeGeneric(GroundType.PlaceHolder6),
                new TemporaryTestBiomeGeneric(GroundType.PlaceHolder7),
                new TemporaryTestBiomeGeneric(GroundType.PlaceHolder8)
            ];

            //Generator generator = new LabyrinthGenerator(width, height, rng.Next());
            Generator generator = new ForestGenerator(width, height, rng.Next());
            tiles = generator.Generate();

            GenerateBiomes(biomes);
            terrainGridTexture = GenerateTerrainTexture(att.GraphicsDevice);
            drawingQuad = CreateVertices();

            List<int> bluenoiseChannels = [0, 1, 2, 3];
            bluenoiseChannels.Shuffle(rng);

            blueNoiseDitherChannel = rng.Next(bluenoiseChannels[0]);
            blueNoiseVariantChannel = rng.Next(bluenoiseChannels[1]);
            blueNoiseSineXChannel = rng.Next(bluenoiseChannels[2]);
            blueNoiseSineYChannel = rng.Next(bluenoiseChannels[3]);
            blueNoiseDitherOffset = new Vector2(rng.Next(TextureLoader.BluenoiseTexture.Width), rng.Next(TextureLoader.BluenoiseTexture.Height));
            blueNoiseVariantOffset = new Vector2(rng.Next(TextureLoader.BluenoiseTexture.Width), rng.Next(TextureLoader.BluenoiseTexture.Height));
            blueNoiseSineXOffset = new Vector2(rng.Next(TextureLoader.BluenoiseTexture.Width), rng.Next(TextureLoader.BluenoiseTexture.Height));
            blueNoiseSineYOffset = new Vector2(rng.Next(TextureLoader.BluenoiseTexture.Width), rng.Next(TextureLoader.BluenoiseTexture.Height));
        }

        private static int CalcDistSqrd(int x1, int y1, int x2, int y2)
        {
            return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
        }

        private static float CalcDist(int x1, int y1, int x2, int y2)
        {
            int distSqrd = CalcDistSqrd(x1, y1, x2, y2);
            return MathF.Sqrt(distSqrd);
        }


        private List<PatchCenter> CreateRing(List<IBiome> biomes)
        {
            var patchCenters = new List<PatchCenter>();

            int repeats = 2;

            float deltaAngle = 360f / (biomes.Count * repeats);

            int xcenter = width / 2;
            int ycenter = height / 2;
            int xradius = width / 4;
            int yradius = height / 4;

            int biomeRadiusSqrd = (width / 6) * (width / 6);

            VoidBiome voidBiome = new();

            /*
            patchCenters.Add(new()
            {
                x = xcenter,
                y = ycenter,
                radius = biome_radius,
                biome = voidBiome
            });
            */

            float spiralDistanceDelta = 0.1f;
            float spiralAngleDeltaMultiplier = 2f;

            for (int i = 0; i < biomes.Count * repeats; i++)
            {
                var angle = new Angle(i * deltaAngle * spiralAngleDeltaMultiplier, AngleType.Degree);
                int x = (int)(xradius * (1 + spiralDistanceDelta * i) * MathF.Cos(angle));
                int y = (int)(yradius * (1 + spiralDistanceDelta * i) * MathF.Sin(angle));

                patchCenters.Add(new()
                {
                    x = xcenter + x,
                    y = ycenter + y,
                    radiusSqrd = biomeRadiusSqrd,
                    biome = biomes[i / repeats]
                });

                /*
                patchCenters.Add(new()
                {
                    x = xcenter + x * 2,
                    y = ycenter + y * 2,
                    radius = biome_radius,
                    biome = voidBiome
                });
                */
            }

            return patchCenters;
        }


        private void GenerateBiomes(List<IBiome> biomes)
        {
            List<PatchCenter> patchCenters = CreateRing(biomes);

            VoidBiome voidBiome = new();

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    float ourDistToMapCenterSqrd = CalcDistSqrd(width / 2, height / 2, x, y);

                    IBiome nearestBiome = voidBiome;

                    float nearestDistSqrd = float.MaxValue;
                    foreach (var patchCenter in patchCenters)
                    {
                        float distSqrd = CalcDistSqrd(x, y, patchCenter.x, patchCenter.y);
                        if (distSqrd < nearestDistSqrd)
                        {
                            float patchCenterRadius = MathF.Sqrt(patchCenter.radiusSqrd);
                            float patchDistToMapCenter = CalcDist(width / 2, height / 2, patchCenter.x, patchCenter.y);
                            float innerBorderDist = patchDistToMapCenter - patchCenterRadius;
                            float outerBorderDist = patchDistToMapCenter + patchCenterRadius;

                            if (ourDistToMapCenterSqrd > innerBorderDist * innerBorderDist
                                && ourDistToMapCenterSqrd < outerBorderDist * outerBorderDist)
                            {
                                nearestBiome = patchCenter.biome;
                                nearestDistSqrd = distSqrd;
                            }
                        }
                    }

                    tiles[x, y].biome = nearestBiome;
                    if (tiles[x, y].isOpen)
                    {
                        tiles[x, y].groundType = nearestBiome.OpenGroundType;
                    }
                    else
                    {
                        tiles[x, y].groundType = nearestBiome.ClosedGroundType;
                    }
                }
            }

            foreach (var patchCenter in patchCenters)
            {
                if (IsInsideGrid(patchCenter.x, patchCenter.y))
                {
                    tiles[patchCenter.x, patchCenter.y].groundType = GroundType.Void;
                }
            }
        }


        private void GenerateBiomesOLD(List<IBiome> biomes, int numPatches)
        {
            biomes.Shuffle(rng);

            int currentBiomeIndex = 0;
            IBiome getNextBiome()
            {
                if (currentBiomeIndex >= biomes.Count)
                {
                    currentBiomeIndex = 0;
                }
                return biomes[currentBiomeIndex++];
            }

            List<PatchCenter> patchCenters = [];
            for (int i = 0; i < numPatches; i++)
            {
                patchCenters.Add(new()
                {
                    x = rng.Next(width),
                    y = rng.Next(height),
                    biome = getNextBiome()
                });
            }

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    IBiome nearestBiome = null;
                    float nearestDistSqrd = float.MaxValue;
                    foreach (var patchCenter in patchCenters)
                    {
                        float distSqrd = CalcDistSqrd(x, y, patchCenter.x, patchCenter.y);
                        if (distSqrd <  nearestDistSqrd)
                        {
                            nearestBiome = patchCenter.biome;
                            nearestDistSqrd = distSqrd;
                        }
                    }
                    tiles[x, y].biome = nearestBiome;
                    if (tiles[x, y].isOpen)
                    {
                        tiles[x, y].groundType = nearestBiome.OpenGroundType;// GroundType.Grass;
                    }
                    else
                    {
                        tiles[x, y].groundType = nearestBiome.ClosedGroundType;//GroundType.ForestFloor;
                    }
                }
            }
        }

        private Texture2D GenerateTerrainTexture(GraphicsDevice graphicsDevice)
        {
            var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Single);
            float[] pixelData = new float[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int pixelIndex = y * width + x;
                    pixelData[pixelIndex] = (byte)tiles[x, y].groundType * 256.0f;
                }
            }
            texture.SetData(pixelData);
            return texture;
        }

        private VertexPositionColorTexture[] CreateVertices()
        {
            var drawingQuad = new VertexPositionColorTexture[4];
            drawingQuad[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0f, 0f));
            drawingQuad[1] = new VertexPositionColorTexture(new Vector3(0, height, 0), Color.Red, new Vector2(0f, 1f));
            drawingQuad[2] = new VertexPositionColorTexture(new Vector3(width, height, 0), Color.Green, new Vector2(1f, 1f));
            drawingQuad[3] = new VertexPositionColorTexture(new Vector3(width, 0, 0), Color.Blue, new Vector2(1f, 0f));
            return drawingQuad;
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            Matrix view = camera.View;
            Matrix projection = camera.Projection;

            EffectLoader.GroundEffect.Parameters["ShadowMap"].SetValue(TextureLoader.ShadowMap);
            EffectLoader.GroundEffect.Parameters["ShadowMapBounds"].SetValue(camera.ShadowRectangle.GetBoundsV4());
            EffectLoader.GroundEffect.Parameters["ShadowMapTileSize"].SetValue(camera.ShadowRectangle.GetSizeV2());

            EffectLoader.GroundEffect.Parameters["WorldViewProjection"].SetValue(view * projection);
            foreach (EffectPass pass in EffectLoader.GroundEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, drawingQuad, 0, 4, drawingIndices, 0, 2);
            }
        }

        internal void Apply()
        {
            EffectLoader.GroundEffect.Parameters["GridSize"].SetValue(new Vector2(width, height));
            EffectLoader.GroundEffect.Parameters["GroundTilesTexture"].SetValue(TextureLoader.GroundTilesTexture);
            EffectLoader.GroundEffect.Parameters["GroundTilesTextureSize"].SetValue(new Vector2(TextureLoader.GroundTilesTexture.Width, TextureLoader.GroundTilesTexture.Height));
            EffectLoader.GroundEffect.Parameters["CellSize"].SetValue((float)CellSize);
            EffectLoader.GroundEffect.Parameters["BlueNoiseTexture"].SetValue(TextureLoader.BluenoiseTexture);
            EffectLoader.GroundEffect.Parameters["BlueNoiseDitherChannel"].SetValue(blueNoiseDitherChannel);
            EffectLoader.GroundEffect.Parameters["BlueNoiseDitherOffset"].SetValue(blueNoiseDitherOffset);
            EffectLoader.GroundEffect.Parameters["BlueNoiseVariantChannel"].SetValue(blueNoiseVariantChannel);
            EffectLoader.GroundEffect.Parameters["BlueNoiseVariantOffset"].SetValue(blueNoiseVariantOffset);
            EffectLoader.GroundEffect.Parameters["BlueNoiseSineXChannel"].SetValue(blueNoiseSineXChannel);
            EffectLoader.GroundEffect.Parameters["BlueNoiseSineXOffset"].SetValue(blueNoiseSineXOffset);
            EffectLoader.GroundEffect.Parameters["BlueNoiseSineYChannel"].SetValue(blueNoiseSineYChannel);
            EffectLoader.GroundEffect.Parameters["BlueNoiseSineYOffset"].SetValue(blueNoiseSineYOffset);
            EffectLoader.GroundEffect.Parameters["BlueNoiseTextureSize"].SetValue(new Vector2(TextureLoader.BluenoiseTexture.Width, TextureLoader.BluenoiseTexture.Height));
            EffectLoader.GroundEffect.Parameters["BlurHalfSize"].SetValue(DitherSize);
            EffectLoader.GroundEffect.Parameters["TerrainGridTexture"].SetValue(terrainGridTexture);
        }

        private bool IsInsideGrid(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private bool HasOpenNeighbours(int x, int y)
        {
            if (x > 0 && tiles[x - 1, y].isOpen)
                return true;

            if (x < (tiles.GetLength(0) - 1) && tiles[x + 1, y].isOpen)
                return true;

            if (y > 0 && tiles[x, y - 1].isOpen)
                return true;

            //if (y < (tiles.GetLength(1) - 1) && tiles[x, y + 1].isOpen)
            //    return true;

            return false;
        }

        internal bool CanSpawnBuilding(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            if (tiles[x, y].isOpen)
                return false;

            return HasOpenNeighbours(x, y);
        }

        internal bool CanSpawnStuff(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            return tiles[x, y].isOpen;
        }

        internal bool CanSpawnTree(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            return !tiles[x, y].isOpen;
        }

        internal bool CanSpawnCritter(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            // TODO: We need to check where stuff is!
            return CanSpawnBuilding(x, y);
        }

        internal bool CanWalkHere(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            if (!tiles[x, y].isOpen)
                return false;

            // TODO: Detect objects!
            return true;
        }

        internal bool IsTreeTile(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            return !tiles[x, y].isOpen;
        }

        internal bool CanSpawnHere(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return false;

            return tiles[x, y].isOpen && tiles[x,y].biome.CanSpawnInThisBiome;
        }

        internal IBiome GetBiome(int x, int y)
        {
            if (!IsInsideGrid(x, y))
                return null;

            return tiles[x, y].biome;
        }
    }
}
