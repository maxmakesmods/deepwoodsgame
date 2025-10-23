using DeepWoods.Game;
using DeepWoods.Graphics;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.World.Biomes;
using DeepWoods.World.Generators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DeepWoods.World
{
    internal class GameWorld
    {
        public static readonly string OverGroundId = "Overground";

        public static string UnderGroundId(int index)
        {
            return $"Underground{index}";
        }

        public static Generator SpiralBiomeGenerator => new SpiralBiomeGenerator();
        public static Generator EightFigureBiomeGenerator => new EightFigureBiomeGenerator();
        public static Generator SingleBiomeGenerator => new SingleBiomeGenerator();
        public static Generator ForestGenerator => new ForestGenerator();
        public static Generator UndergroundGenerator => new UndergroundGenerator();
        public static Generator GroundTypeGenerator => new GroundTypeGenerator();

        private AllTheThings ATT { get; set; }

        private readonly List<BiomePair> biomePairs;

        public Dictionary<string, SubWorld> SubWorlds { get; private set; } = [];
        private readonly Random rng;

        private readonly Dictionary<PlayerIndex, SubWorld> playerSubWorlds = new();
        public int Seed { get; private set; }

        public GameWorld(AllTheThings att, int seed, int width, int height)
        {
            ATT = att;
            Seed = seed;
            rng = new Random(seed);

            // TODO: actual biomes
            biomePairs = TemporaryGetHardcodedBiomes();

            var overground = new SubWorld(att, rng.Next(), width, height, biomePairs.Select(b => b.Overground).ToList(), SpiralBiomeGenerator, ForestGenerator, GroundTypeGenerator);
            SubWorlds.Add(OverGroundId, overground);
        
            for (int i = 0; i < biomePairs.Count; i++)
            {
                if (biomePairs[i].Underground != null)
                {
                    SubWorld underground = null;
                    var biomeRectangle = overground.Terrain.GetBiomeRectangle(biomePairs[i].Overground);
                    bool cavesGenerated = false;
                    int counter = 0;
                    while (!cavesGenerated)
                    {
                        underground = new SubWorld(att, rng.Next(), biomeRectangle.Width, biomeRectangle.Height, [biomePairs[i].Underground], SingleBiomeGenerator, UndergroundGenerator, GroundTypeGenerator);
                        cavesGenerated = overground.ObjectManager.GenerateCaves(biomePairs[i].Overground, biomeRectangle, underground, 10);
                        counter++;
                    }
                    Debug.WriteLine($"counter: {counter}");
                    SubWorlds.Add(UnderGroundId(i), underground);
                }
            }

            foreach (var (_, subworld) in SubWorlds)
            {
                subworld.ObjectManager.FinalGenerate();
            }
        }

        private static List<BiomePair> TemporaryGetHardcodedBiomes()
        {
            return [
                new BiomePair(new TemperateForestBiome(), new GenericBiome(GroundType.Sand, GroundType.Mud)),
                new BiomePair(new GenericBiome(1) { Trees = ["tree3", "tree4"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
                new BiomePair(new GenericBiome(2) { Trees = ["swamp_tree1", "swamp_tree2"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
                new BiomePair(new GenericBiome(3) { Trees = ["volcanic_tree1", "volcanic_tree2"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
                new BiomePair(new GenericBiome(4) { Trees = ["lake_tree1", "lake_tree2"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
                //new BiomePair(new GenericBiome(5) { Trees = ["lakeunderwater_tree1", "lakeunderwater_tree2"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
                new BiomePair(new GenericBiome(6) { Trees = ["borealforest_tree1", "borealforest_tree2"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
                new BiomePair(new GenericBiome(7) { Trees = ["magic_tree1", "magic_tree2", "magic_tree1_color2", "magic_tree2_color2"] }, new GenericBiome(GroundType.Sand, GroundType.Mud)),
            ];
        }

        internal void SwitchToOverground(Player player)
        {
            SwitchSubWorld(player, OverGroundId);
        }

        internal void SwitchToUnderground(Player player, int index)
        {
            SwitchSubWorld(player, UnderGroundId(index));
        }

        private void SwitchSubWorld(Player player, string id)
        {
            if (SubWorlds.TryGetValue(id, out SubWorld subWorld))
            {
                playerSubWorlds[player.PlayerIndex] = subWorld;
            }
        }

        private SubWorld GetPlayerSubWorld(Player player)
        {
            if (playerSubWorlds.TryGetValue(player.PlayerIndex, out SubWorld subWorld))
            {
                return subWorld;
            }
            return SubWorlds[OverGroundId];
        }

        internal bool IsCave(Player player, int currentTileX, int currentTileY)
        {
            var subWorld = GetPlayerSubWorld(player);
            return subWorld.ObjectManager.IsCave(currentTileX, currentTileY);
        }

        internal DWObject TryPickUpObject(Player player, int currentTileX, int currentTileY)
        {
            var subWorld = GetPlayerSubWorld(player);
            return subWorld.ObjectManager.TryPickUpObject(currentTileX, currentTileY);
        }

        internal Point GetSpawnPosition()
        {
            return SubWorlds[OverGroundId].Terrain.GetSpawnPosition();
        }

        internal void Draw(GraphicsDevice graphicsDevice, Player player)
        {
            var subWorld = GetPlayerSubWorld(player);
            subWorld.Apply();
            subWorld.ObjectManager.DrawShadowMap(ATT.GraphicsDevice, ATT.PlayerManager.Players, player);
            ATT.GraphicsDevice.SetRenderTarget(player.myRenderTarget);
            ATT.GraphicsDevice.Clear(DWRenderer.ClearColor);
            ATT.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            subWorld.Terrain.Draw(ATT.GraphicsDevice, player);
            ATT.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            subWorld.ObjectManager.Draw(ATT.GraphicsDevice, player);
        }

        internal Terrain GetTerrain(Player player)
        {
            var subWorld = GetPlayerSubWorld(player);
            return subWorld.Terrain;
        }

        internal void Update(double dayDelta, float deltaTime)
        {
            foreach (var (_, subWorld) in SubWorlds)
            {
                subWorld.Update(dayDelta, deltaTime);
            }
        }

        internal void SwitchOverUnderground(Player player, int caveX, int caveY)
        {
            int oppositeCaveX, oppositeCaveY;
            var subWorld = GetPlayerSubWorld(player);
            IBiome biome = subWorld.Terrain.GetBiome(caveX, caveY);
            if (subWorld == SubWorlds[OverGroundId])
            {
                var biomeRectangle = subWorld.Terrain.GetBiomeRectangle(biome);
                int biomeIndex = biomePairs.FindIndex(p => p.Overground == biome);
                SwitchToUnderground(player, biomeIndex);
                oppositeCaveX = caveX - biomeRectangle.X;
                oppositeCaveY = caveY - biomeRectangle.Y;
            }
            else
            {
                int biomeIndex = biomePairs.FindIndex(p => p.Underground == biome);
                var biomeRectangle = SubWorlds[OverGroundId].Terrain.GetBiomeRectangle(biomePairs[biomeIndex].Overground);
                SwitchToOverground(player);
                oppositeCaveX = caveX + biomeRectangle.X;
                oppositeCaveY = caveY + biomeRectangle.Y;
            }

            subWorld = GetPlayerSubWorld(player);
            Point[] directions = [new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)];
            foreach (var direction in directions)
            {
                if (subWorld.Terrain.CanWalkHere(oppositeCaveX + direction.X, oppositeCaveY + direction.Y))
                {
                    player.position = new Vector2(oppositeCaveX + direction.X, oppositeCaveY + direction.Y);
                    break;
                }
            }
        }
    }
}
