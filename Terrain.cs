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

        public static GroundType[,] GenerateTerrain(int width, int height)
        {
            var groundTypes = Enum.GetValues<GroundType>();
            var rng = new Random();

            GroundType[,] grid = new GroundType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = groundTypes[rng.Next(groundTypes.Length)];
                }
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
