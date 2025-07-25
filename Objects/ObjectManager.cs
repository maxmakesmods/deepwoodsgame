using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Players;
using DeepWoods.World;
using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static DeepWoods.World.Terrain;

namespace DeepWoods.Objects
{
    internal class ObjectManager
    {
        private enum Critter
        {
            CROW,
            HEDGEHOG,
            BEEHIVE,
            FROG
        }

        private readonly List<DWObject> objectTypes;
        private readonly Random rng;
        private readonly int width;
        private readonly int height;

        private InstancedObjects instancedObjects;
        private InstancedObjects instancedCritters;

        public ObjectManager(ContentManager content, GraphicsDevice graphicsDevice, int seed, int width, int height, Terrain terrain)
        {
            rng = new Random(seed);
            objectTypes = content.Load<List<DWObject>>("objects/objects");

            this.width = width;
            this.height = height;

            TemperateForestBiome biome = new TemperateForestBiome();


            List<Sprite> objects = new List<Sprite>();
            List<Sprite> critters = new List<Sprite>();

            GenerateObjects(biome, terrain, objects, critters);

            instancedObjects = new InstancedObjects(graphicsDevice, objects, TextureLoader.ObjectsTexture);
            instancedCritters = new InstancedObjects(graphicsDevice, critters, TextureLoader.Critters);
        }

        private void GenerateObjects(IBiome biome, Terrain terrain, List<Sprite> objects, List<Sprite> critters)
        {
            var critterIDs = new List<Critter>(Enum.GetValues<Critter>());

            // TODO TEMP Sprite Test
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (terrain.CanSpawnCritter(x, y) && rng.NextSingle() < biome.StuffDensity)
                    {
                        Critter critter = critterIDs[rng.Next(critterIDs.Count)];
                        critters.Add(new Sprite(new Vector2(x, y), new Rectangle(32 * (int)critter, 0, 32, 32), true, false, 8, 32, 4));
                    }
                    else if (terrain.CanSpawnStuff(x, y) && rng.NextSingle() < biome.StuffDensity)
                    {
                        var o = SpawnRandomObject(biome.Stuff, x, y);
                        if (o != null)
                        {
                            objects.Add(o);
                        }
                    }
                    else if (terrain.CanSpawnBuilding(x, y) && rng.NextSingle() < biome.BuildingDensity)
                    {
                        var o = SpawnRandomObject(biome.Buildings, x, y);
                        if (o != null)
                        {
                            objects.Add(o);
                        }
                    }
                    else if (terrain.CanSpawnTree(x, y))
                    {
                        var o = SpawnRandomObject(biome.Trees, x, y);
                        if (o != null)
                        {
                            objects.Add(o);
                        }
                    }
                }
            }

            /*
            SpawnObject("tree1", 2, 3);
            SpawnObject("tree1", 3, 3);
            SpawnObject("tree1", 4, 3);
            SpawnObject("tree1", 5, 3);
            SpawnObject("tree1", 6, 3);
            SpawnObject("tree1", 7, 3);
            SpawnObject("tower", 5, 2);
            */
        }

        private Sprite SpawnRandomObject(List<string> objectList, int x, int y)
        {
            if (objectList.Count == 0)
            {
                return null;
            }
            var objectName = objectList[rng.Next(objectList.Count)];
            return SpawnObject(objectName, x, y);
        }

        private Sprite SpawnObject(string name, int x, int y)
        {
            var dwobj = objectTypes.Where(o => o.name == name).FirstOrDefault();
            if (dwobj == null)
            {
                return null;
            }
            return new Sprite(new Vector2(x, y), new Rectangle(dwobj.x, dwobj.y, dwobj.width, dwobj.height), dwobj.standing, dwobj.glowing);
        }


        internal void DrawShadowMap(GraphicsDevice graphicsDevice, List<Player> players, Camera camera)
        {
            Matrix view = camera.ShadowView;
            Matrix projection = camera.ShadowProjection;

            graphicsDevice.SetRenderTarget(TextureLoader.ShadowMap);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            EffectLoader.SpriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            EffectLoader.SpriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            EffectLoader.SpriteEffect.Parameters["IsShadow"].SetValue(1);


            instancedObjects.Draw(graphicsDevice);
            instancedCritters.Draw(graphicsDevice);


            foreach (var player in players)
            {
                player.DrawShadow(graphicsDevice, camera);
            }

            graphicsDevice.SetRenderTarget(null);
        }


        internal void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            Matrix view = camera.View;
            Matrix projection = camera.Projection;

            var spriteEffect = EffectLoader.SpriteEffect;

            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["IsShadow"].SetValue(0);
            spriteEffect.Parameters["ShadowMap"].SetValue(TextureLoader.ShadowMap);
            spriteEffect.Parameters["ShadowMapBounds"].SetValue(camera.ShadowRectangle.GetBoundsV4());
            spriteEffect.Parameters["ShadowMapTileSize"].SetValue(camera.ShadowRectangle.GetSizeV2());

            instancedObjects.Draw(graphicsDevice);
            instancedCritters.Draw(graphicsDevice);
        }
    }
}