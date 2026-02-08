using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Main;
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
    public class ObjectManager
    {
        private readonly List<DWObjectDefinition> objectDefinitions;
        private readonly Random rng;

        private readonly Terrain terrain;

        private readonly int width;
        private readonly int height;

        private readonly List<DWObject> objects = [];
        private readonly List<DWObject> critters = [];
        private readonly Dictionary<(int, int), int> objectIndices = [];

        private InstancedObjects instancedObjects;
        private InstancedObjects instancedCritters;

        public ObjectManager(Terrain terrain, int seed)
        {
            this.terrain = terrain;
            rng = new Random(seed);
            objectDefinitions = DeepWoodsMain.Instance.Content.Load<List<DWObjectDefinition>>("objects/objects");

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
                instancedObjects = new InstancedObjects(rng.Next(), objects, TextureLoader.ObjectsTexture, TextureLoader.ObjectsGlowMap);
            }
            if (critters.Count > 0)
            {
                instancedCritters = new InstancedObjects(rng.Next(), critters, TextureLoader.Critters, TextureLoader.BlackTexture);
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


        internal void DrawShadowMap(List<Player> players, LocalPlayer player)
        {
            DeepWoodsMain.Instance.GraphicsDevice.SetRenderTarget(player.ShadowMap);
            DeepWoodsMain.Instance.GraphicsDevice.Clear(Color.Black);
            DeepWoodsMain.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            DeepWoodsMain.Instance.GraphicsDevice.BlendState = BlendState.Opaque;

            Matrix view = player.Camera.ShadowView;
            Matrix projection = player.Camera.ShadowProjection;

            EffectLoader.SpriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            EffectLoader.SpriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            EffectLoader.SpriteEffect.Parameters["IsShadow"].SetValue(1);

            instancedObjects?.Draw();
            instancedCritters?.Draw();

            foreach (var pl in players)
            {
                pl.DrawShadow(player.Camera);
            }

            DeepWoodsMain.Instance.GraphicsDevice.SetRenderTarget(null);
        }


        internal void Draw(LocalPlayer player)
        {
            DeepWoodsMain.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            DeepWoodsMain.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix view = player.Camera.View;
            Matrix projection = player.Camera.Projection;

            var spriteEffect = EffectLoader.SpriteEffect;

            spriteEffect.Parameters["CellSize"].SetValue(Terrain.CellSize);
            spriteEffect.Parameters["ViewProjection"].SetValue(view * projection);
            spriteEffect.Parameters["IsShadow"].SetValue(0);
            spriteEffect.Parameters["ShadowMap"].SetValue(player.ShadowMap);
            spriteEffect.Parameters["ShadowMapBounds"].SetValue(player.Camera.ShadowRectangle.GetBoundsV4());
            spriteEffect.Parameters["ShadowMapTileSize"].SetValue(player.Camera.ShadowRectangle.GetSizeV2());

            instancedObjects?.Draw();
            instancedCritters?.Draw();
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