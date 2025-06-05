using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods
{
    internal class Terrain
    {
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

        private static int calcDistSqrd(int x1, int y1, int x2, int y2)
        {
            return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
        }

        public static GroundType[,] GenerateTerrain(int width, int height, int numPatches)
        {
            var rng = new Random();
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

            // TODO: temporary debug
            foreach (var patchCenter in patchCenters)
            {
                grid[patchCenter.x, patchCenter.y] = (GroundType)5;
            }

            return grid;
        }

        public static Texture2D GenerateTerrainTexture(GraphicsDevice graphicsDevice, GroundType[,] grid)
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
    }
}
