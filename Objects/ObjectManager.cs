using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepWoods.Loaders;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.Objects
{
    internal class ObjectManager
    {
        private List<Sprite> sprites = new List<Sprite>();

        private Random rng;
        private int width;
        private int height;

        public ObjectManager(int seed, int width, int height, Terrain terrain)
        {
            rng = new Random(seed);

            // TODO TEMP Sprite Test
            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    if (terrain.terrainGrid[x, y] == Terrain.GroundType.Grass)
                    {
                        if (rng.NextSingle() < 0.9f)
                        {
                            sprites.Add(new Sprite(TextureLoader.TreeTexture, new Vector2(x, y), new Vector2(1, 2), true));
                        }
                        else
                        {
                            if (rng.NextSingle() < 0.5f)
                            {
                                sprites.Add(new Sprite(TextureLoader.TowerTexture, new Vector2(x, y), new Vector2(1, 2), true));
                            }
                            else
                            {
                                sprites.Add(new Sprite(TextureLoader.WagonTexture, new Vector2(x, y), new Vector2(1, 1), true));
                            }
                        }
                    }
                    else
                    {
                        if (rng.NextSingle() < 0.05f)
                        {
                            sprites.Add(new Sprite(TextureLoader.CampfireAbandonedTexture, new Vector2(x, y), new Vector2(1, 1), false));
                        }
                    }
                }
            }

        }

        internal void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            foreach (var sprite in sprites)
            {
                sprite.Draw(graphicsDevice, EffectLoader.SpriteEffect, view, projection);
            }
        }
    }
}
