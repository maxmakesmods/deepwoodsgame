using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DeepWoods.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.World
{
    internal class Terrain
    {
        public readonly static int CellSize = 32;
        private readonly static int DitherSize = 2;

        private readonly VertexPositionColorTexture[] drawingQuad;
        private readonly short[] drawingIndices = [0, 1, 2, 0, 2, 3];
        public readonly GroundType[,] terrainGrid; // TODO: make private
        private readonly Texture2D terrainGridTexture;
        public readonly int seed; // TODO: make private
        private readonly int width;
        private readonly int height;
        private readonly int blueNoiseDitherChannel;
        private readonly Vector2 blueNoiseDitherOffset;
        private readonly int blueNoiseVariantChannel;
        private readonly Vector2 blueNoiseVariantOffset;
        private readonly Random rng;

        public enum GroundType
        {
            Grass, Sand, Mud, Gravel
        }

        private class PatchCenter
        {
            public int x;
            public int y;
            public GroundType groundType;
        }

        public Terrain(GraphicsDevice graphicsDevice, int seed, int width, int height, int numPatches)
        {
            rng = new Random(seed);

            terrainGrid = GenerateTerrain(width, height, numPatches);
            terrainGridTexture = GenerateTerrainTexture(graphicsDevice, terrainGrid);
            drawingQuad = CreateVertices(width, height);
            this.seed = seed;
            this.width = width;
            this.height = height;

            blueNoiseDitherChannel = rng.Next(4);
            blueNoiseVariantChannel = rng.Next(4);
            blueNoiseDitherOffset = new Vector2(rng.Next(TextureLoader.BluenoiseTexture.Width), rng.Next(TextureLoader.BluenoiseTexture.Height));
            blueNoiseVariantOffset = new Vector2(rng.Next(TextureLoader.BluenoiseTexture.Width), rng.Next(TextureLoader.BluenoiseTexture.Height));
        }

        private static int calcDistSqrd(int x1, int y1, int x2, int y2)
        {
            return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
        }

        private GroundType[,] GenerateTerrain(int width, int height, int numPatches)
        {
            var groundTypes = Enum.GetValues<GroundType>().ToList();
            groundTypes.Sort((g1, g2) => rng.Next(-1, 2));

            int currentGroundTypeIndex = 0;

            GroundType getNextGroundType()
            {
                if (currentGroundTypeIndex >= groundTypes.Count)
                {
                    currentGroundTypeIndex = 0;
                }
                return groundTypes[currentGroundTypeIndex++];
            }

            List<PatchCenter> patchCenters = new();
            for (int i = 0; i < numPatches; i++)
            {
                patchCenters.Add(new()
                {
                    x = rng.Next(width),
                    y = rng.Next(height),
                    groundType = getNextGroundType()
                });
            }

            GroundType[,] grid = new GroundType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GroundType nearestGroundType = GroundType.Grass;
                    float nearestDistSqrd = float.MaxValue;
                    foreach (var patchCenter in patchCenters)
                    {
                        float distSqrd = calcDistSqrd(x, y, patchCenter.x, patchCenter.y);
                        if (distSqrd <  nearestDistSqrd)
                        {
                            nearestGroundType = patchCenter.groundType;
                            nearestDistSqrd = distSqrd;
                        }
                    }
                    grid[x, y] = nearestGroundType;
                }
            }

            return grid;
        }

        private Texture2D GenerateTerrainTexture(GraphicsDevice graphicsDevice, GroundType[,] grid)
        {
            Texture2D texture = new Texture2D(graphicsDevice, grid.GetLength(0), grid.GetLength(1), false, SurfaceFormat.Single);
            float[] pixelData = new float[grid.GetLength(0) * grid.GetLength(1)];
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    int pixelIndex = y * grid.GetLength(0) + x;
                    pixelData[pixelIndex] = (byte)grid[x, y] * 256.0f;
                }
            }
            texture.SetData(pixelData);
            return texture;
        }

        private VertexPositionColorTexture[] CreateVertices(int width, int height)
        {
            var drawingQuad = new VertexPositionColorTexture[4];
            drawingQuad[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0f, 0f));
            drawingQuad[1] = new VertexPositionColorTexture(new Vector3(0, height, 0), Color.Red, new Vector2(0f, 1f));
            drawingQuad[2] = new VertexPositionColorTexture(new Vector3(width, height, 0), Color.Green, new Vector2(1f, 1f));
            drawingQuad[3] = new VertexPositionColorTexture(new Vector3(width, 0, 0), Color.Blue, new Vector2(1f, 0f));
            return drawingQuad;
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
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
            EffectLoader.GroundEffect.Parameters["BlueNoiseTextureSize"].SetValue(new Vector2(TextureLoader.BluenoiseTexture.Width, TextureLoader.BluenoiseTexture.Height));
            EffectLoader.GroundEffect.Parameters["BlurHalfSize"].SetValue(DitherSize);
            EffectLoader.GroundEffect.Parameters["TerrainGridTexture"].SetValue(terrainGridTexture);
        }
    }
}
