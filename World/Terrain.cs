using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Main;
using DeepWoods.Players;
using DeepWoods.World.Biomes;
using DeepWoods.World.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepWoods.World
{
    public class Terrain
    {
        private readonly static int FogScale = 1;

        public readonly static int CellSize = 32;
        private readonly static int DitherSize = 4;

        private readonly VertexPositionColorTexture[] drawingQuad;
        private readonly short[] drawingIndices = [0, 1, 2, 0, 2, 3];
        private readonly Tile[,] tiles;

        private readonly Texture2D gridTexture;
        private readonly Dictionary<PlayerId, Texture2D> fogLayers = [];

        private readonly int seed;
        private readonly int width;
        private readonly int height;
        private readonly int renderwidth;
        private readonly int renderheight;
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

        public Terrain(int seed, int width, int height,
            List<IBiome> biomes,
            Generator biomeGenerator,
            Generator forestGenerator,
            Generator groundTypeGenerator)
        {
            rng = new Random(seed);
            this.seed = seed;
            this.width = width;
            this.height = height;

            this.renderwidth = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)width);
            this.renderheight = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)height);

            tiles = new Tile[width, height];

            biomeGenerator.Generate(tiles, biomes, rng.Next());
            forestGenerator.Generate(tiles, biomes, rng.Next());
            groundTypeGenerator.Generate(tiles, biomes, rng.Next());

            gridTexture = GenerateTerrainTexture();
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

        private Texture2D GenerateTerrainTexture()
        {
            var texture = new Texture2D(DeepWoodsMain.Instance.GraphicsDevice, renderwidth, renderheight, false, SurfaceFormat.Single);
            float[] pixelData = new float[renderwidth * renderheight];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int pixelIndex = y * renderwidth + x;
                    pixelData[pixelIndex] = (byte)tiles[x, y].groundType * 256.0f;
                }
            }
            texture.SetData(pixelData);
            return texture;
        }

        private Texture2D GetFogLayer(LocalPlayer player)
        {
            if (!fogLayers.TryGetValue(player.ID, out var fogLayer))
            {
                fogLayer = new Texture2D(DeepWoodsMain.Instance.GraphicsDevice, renderwidth * FogScale, renderheight * FogScale, false, SurfaceFormat.Alpha8);
                fogLayer.SetData(new byte[fogLayer.Width * fogLayer.Height]);
                fogLayers[player.ID] = fogLayer;
                UpdateFogLayer(player);
            }
            return fogLayer;
        }

        public Texture2D TempDebugGetFogLayer()
        {
            return fogLayers.Values.FirstOrDefault();
        }

        public void UpdateFogLayer(Player player)
        {
            if (fogLayers.TryGetValue(player.ID, out var fogLayer))
            {
                byte[] pixelData = new byte[fogLayer.Width * fogLayer.Height];
                fogLayer.GetData(pixelData);

                Point pos = (player.position * FogScale).RoundToPoint();
                int radius = player.ViewDistance * FogScale;

                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        float length = new Vector2(x, y).Length();
                        if (length <= radius)
                        {
                            Point npos = pos + new Point(x, y);
                            int pixelIndex = npos.Y * fogLayer.Width + npos.X;
                            if (pixelIndex >= 0 && pixelIndex < pixelData.Length)
                            {
                                if (length + 1 < radius)
                                {
                                    pixelData[pixelIndex] = 255;
                                }
                                else if (pixelData[pixelIndex] < 255)
                                {
                                    pixelData[pixelIndex] = 128;
                                }
                            }
                        }
                    }
                }

                fogLayer.SetData(pixelData);
            }
        }

        private VertexPositionColorTexture[] CreateVertices()
        {
            var drawingQuad = new VertexPositionColorTexture[4];
            drawingQuad[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0f, 0f));
            drawingQuad[1] = new VertexPositionColorTexture(new Vector3(0, renderheight, 0), Color.Red, new Vector2(0f, 1f));
            drawingQuad[2] = new VertexPositionColorTexture(new Vector3(renderwidth, renderheight, 0), Color.Green, new Vector2(1f, 1f));
            drawingQuad[3] = new VertexPositionColorTexture(new Vector3(renderwidth, 0, 0), Color.Blue, new Vector2(1f, 0f));
            return drawingQuad;
        }

        public void Draw(LocalPlayer player)
        {
            Matrix view = player.myCamera.View;
            Matrix projection = player.myCamera.Projection;

            EffectLoader.SetParamSafely("ShadowMap", player.myShadowMap);
            EffectLoader.SetParamSafely("ShadowMapBounds", player.myCamera.ShadowRectangle.GetBoundsV4());
            EffectLoader.SetParamSafely("ShadowMapTileSize", player.myCamera.ShadowRectangle.GetSizeV2());

            EffectLoader.SetParamSafely("WorldViewProjection", view * projection);
            foreach (EffectPass pass in EffectLoader.GroundEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                DeepWoodsMain.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, drawingQuad, 0, 4, drawingIndices, 0, 2);
            }
        }

        internal void Apply(LocalPlayer player)
        {
            EffectLoader.SetParamSafely("GridSize", new Vector2(renderwidth, renderheight));
            EffectLoader.SetParamSafely("CellSize", (float)CellSize);
            EffectLoader.SetParamSafely("GroundTilesTexture", TextureLoader.GroundTilesTexture);
            EffectLoader.SetParamSafely("GroundTilesTextureSize", new Vector2(TextureLoader.GroundTilesTexture.Width, TextureLoader.GroundTilesTexture.Height));
            EffectLoader.SetParamSafely("GlowMap", TextureLoader.GroundTilesGlowMap);
            EffectLoader.SetParamSafely("BlueNoiseTexture", TextureLoader.BluenoiseTexture);
            EffectLoader.SetParamSafely("BlueNoiseDitherChannel", blueNoiseDitherChannel);
            EffectLoader.SetParamSafely("BlueNoiseDitherOffset", blueNoiseDitherOffset);
            EffectLoader.SetParamSafely("BlueNoiseVariantChannel", blueNoiseVariantChannel);
            EffectLoader.SetParamSafely("BlueNoiseVariantOffset", blueNoiseVariantOffset);
            EffectLoader.SetParamSafely("BlueNoiseSineXChannel", blueNoiseSineXChannel);
            EffectLoader.SetParamSafely("BlueNoiseSineXOffset", blueNoiseSineXOffset);
            EffectLoader.SetParamSafely("BlueNoiseSineYChannel", blueNoiseSineYChannel);
            EffectLoader.SetParamSafely("BlueNoiseSineYOffset", blueNoiseSineYOffset);
            EffectLoader.SetParamSafely("BlueNoiseTextureSize", new Vector2(TextureLoader.BluenoiseTexture.Width, TextureLoader.BluenoiseTexture.Height));
            EffectLoader.SetParamSafely("BlurHalfSize", DitherSize);
            EffectLoader.SetParamSafely("TerrainGridTexture", gridTexture);
            EffectLoader.SetParamSafely("TerrainFogLayer", GetFogLayer(player));
            EffectLoader.SetParamSafely("DUDVTexture", TextureLoader.DUDVTexture);
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

            if (tiles[x, y].biome == null || tiles[x, y].biome.IsVoid)
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

        internal Point GetSpawnPosition()
        {
            int spawnX = width / 2;
            int spawnY = height / 2;
            while (!CanSpawnHere(spawnX, spawnY))
            {
                spawnX = spawnX + rng.Next(-1, 2);
                spawnY = spawnY + rng.Next(-1, 2);
            }
            return new Point(spawnX, spawnY);
        }

        internal Rectangle GetBiomeRectangle(IBiome biome)
        {
            bool foundAny = false;

            int xstart = int.MaxValue;
            int ystart = int.MaxValue;
            int xend = int.MinValue;
            int yend = int.MinValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y].biome == biome)
                    {
                        foundAny = true;
                        xstart = Math.Min(x, xstart);
                        ystart = Math.Min(y, ystart);
                        xend = Math.Max(x, xend);
                        yend = Math.Max(y, yend);
                    }
                }
            }

            if (!foundAny)
            {
                return Rectangle.Empty;
            }

            return new Rectangle(xstart, ystart, xend - xstart, yend - ystart);
        }
    }
}
