using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Players;
using DeepWoods.World;
using DeepWoods.World.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepWoods.Objects
{
    internal class ObjectManager
    {
        private readonly List<DWObjectDefinition> objectDefinitions;
        private readonly Random rng;

        private AllTheThings ATT { get; set; }

        private readonly Terrain terrain;

        private readonly int width;
        private readonly int height;

        private readonly List<DWObject> objects = [];
        private readonly List<DWObject> critters = [];
        private readonly Dictionary<(int, int), int> objectIndices = [];

        private InstancedObjects instancedObjects;
        private InstancedObjects instancedCritters;

        public ObjectManager(AllTheThings att, Terrain terrain, int seed)
        {
            ATT = att;
            this.terrain = terrain;
            rng = new Random(seed);
            objectDefinitions = att.Content.Load<List<DWObjectDefinition>>("objects/objects");

            width = terrain.Width;
            height = terrain.Height;
        }

        internal bool GenerateCaves(IBiome overgroundBiome, Rectangle rectangle, SubWorld underground, int numCaves)
        {
            List<Point> possibleCavePositions = new();

            for (int x = 0; x < rectangle.Width; x++)
            {
                for (int y = 0; y < rectangle.Height; y++)
                {
                    if (terrain.GetBiome(rectangle.X + x, rectangle.Y + y) == overgroundBiome
                        && terrain.CanSpawnBuilding(rectangle.X + x, rectangle.Y + y))
                    {
                        if (underground.Terrain.CanSpawnBuilding(x, y))
                        {
                            possibleCavePositions.Add(new Point(x, y));
                        }
                    }
                }
            }

            if (possibleCavePositions.Count < numCaves)
            {
                return false;
            }

            possibleCavePositions.Shuffle(rng);

            for (int i = 0; i < numCaves; i++)
            {
                Point cavePos = possibleCavePositions[i];
                SpawnCave(rectangle.X + cavePos.X, rectangle.Y + cavePos.Y);
                underground.ObjectManager.SpawnCave(cavePos.X, cavePos.Y);
            }

            return true;
        }

        protected void SpawnCave(int x, int y)
        {
            IBiome biome = terrain.GetBiome(x, y);
            if (biome == null)
            {
                return;
            }
            var cave = SpawnObject(biome.CaveObjectId, x, y);
            if (cave != null)
            {
                objectIndices.Add((x, y), objects.Count);
                objects.Add(cave);
            }
        }

        internal void FinalGenerate()
        {
            GenerateObjects(terrain, objects, critters);

            if (objects.Count > 0)
            {
                instancedObjects = new InstancedObjects(ATT.GraphicsDevice, objects, TextureLoader.ObjectsTexture);
            }
            if (critters.Count > 0)
            {
                instancedCritters = new InstancedObjects(ATT.GraphicsDevice, critters, TextureLoader.Critters);
            }
        }

        private void GenerateObjects(Terrain terrain, List<DWObject> objects, List<DWObject> critters)
        {
            // TODO TEMP Sprite Test
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    IBiome biome = terrain.GetBiome(x, y);
                    if (biome == null)
                    {
                        continue;
                    }
                    if (objectIndices.ContainsKey((x, y)))
                    {
                        continue;
                    }
                    if (terrain.CanSpawnCritter(x, y) && rng.NextSingle() < biome.StuffDensity)
                    {
                        var c = SpawnRandomCritter(biome.Critters, x, y);
                        if (c != null)
                        {
                            critters.Add(c);
                        }
                    }
                    else if (terrain.CanSpawnStuff(x, y) && rng.NextSingle() < biome.StuffDensity)
                    {
                        var o = SpawnRandomObject(biome.Stuff, x, y);
                        if (o != null)
                        {
                            objectIndices.Add((x, y), objects.Count);
                            objects.Add(o);
                        }
                    }
                    else if (terrain.CanSpawnBuilding(x, y) && rng.NextSingle() < biome.BuildingDensity)
                    {
                        var o = SpawnRandomObject(biome.Buildings, x, y);
                        if (o != null)
                        {
                            objectIndices.Add((x, y), objects.Count);
                            objects.Add(o);
                        }
                    }
                    else if (terrain.CanSpawnTree(x, y))
                    {
                        var o = SpawnRandomObject(biome.Trees, x, y);
                        if (o != null)
                        {
                            objectIndices.Add((x, y), objects.Count);
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

        private DWObject SpawnRandomCritter(List<CritterDefinitions.Critter> critters, int x, int y)
        {
            if (critters.Count == 0)
            {
                return null;
            }
            CritterDefinitions.Critter critter = critters[rng.Next(critters.Count)];
            var def = CritterDefinitions.GetCritterDefinition(critter);
            return new DWObject(new Vector2(x, y), def);
        }

        private DWObject SpawnRandomObject(List<string> objectList, int x, int y)
        {
            if (objectList.Count == 0)
            {
                return null;
            }
            var objectName = objectList[rng.Next(objectList.Count)];
            return SpawnObject(objectName, x, y);
        }

        private DWObject SpawnObject(string name, int x, int y)
        {
            var def = objectDefinitions.Where(o => o.Name == name).FirstOrDefault();
            if (def == null)
            {
                return null;
            }
            return new DWObject(new Vector2(x, y), def);
        }


        internal void DrawShadowMap(GraphicsDevice graphicsDevice, List<Player> players, Player player)
        {
            Matrix view = player.myCamera.ShadowView;
            Matrix projection = player.myCamera.ShadowProjection;

            graphicsDevice.SetRenderTarget(player.myShadowMap);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            EffectLoader.SpriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            EffectLoader.SpriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            EffectLoader.SpriteEffect.Parameters["IsShadow"].SetValue(1);


            instancedObjects?.Draw(graphicsDevice);
            instancedCritters?.Draw(graphicsDevice);


            foreach (var pl in players)
            {
                pl.DrawShadow(graphicsDevice, player.myCamera);
            }

            graphicsDevice.SetRenderTarget(null);
        }


        internal void Draw(GraphicsDevice graphicsDevice, Player player)
        {
            Matrix view = player.myCamera.View;
            Matrix projection = player.myCamera.Projection;

            var spriteEffect = EffectLoader.SpriteEffect;

            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["IsShadow"].SetValue(0);
            spriteEffect.Parameters["ShadowMap"].SetValue(player.myShadowMap);
            spriteEffect.Parameters["ShadowMapBounds"].SetValue(player.myCamera.ShadowRectangle.GetBoundsV4());
            spriteEffect.Parameters["ShadowMapTileSize"].SetValue(player.myCamera.ShadowRectangle.GetSizeV2());

            instancedObjects?.Draw(graphicsDevice);
            instancedCritters?.Draw(graphicsDevice);
        }

        internal bool IsCave(int x, int y)
        {
            if (objectIndices.TryGetValue((x, y), out var index))
            {
                return objects[index].Def.Name == "crystal ball";
            }

            return false;
        }

        internal DWObject TryPickUpObject(int x, int y)
        {
            if (terrain.IsTreeTile(x, y))
            {
                return null;
            }

            if (IsCave(x, y))
            {
                return null;
            }

            if (objectIndices.TryGetValue((x,y), out var index))
            {
                instancedObjects?.HideInstance(index);
                objectIndices.Remove((x, y));
                return objects[index];
            }

            return null;
        }
    }
}