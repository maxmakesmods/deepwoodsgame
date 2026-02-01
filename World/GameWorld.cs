using DeepWoods.Game;
using DeepWoods.Graphics;
using DeepWoods.Helpers;
using DeepWoods.Main;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.World.Biomes;
using DeepWoods.World.Generators;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DeepWoods.World
{
    public class GameWorld
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

        private readonly List<BiomePair> biomePairs;

        public Dictionary<string, SubWorld> SubWorlds { get; private set; } = [];
        private readonly Random rng;

        private readonly Dictionary<PlayerId, SubWorld> playerSubWorlds = new();
        private readonly DeepWoodsGame game;

        public int Seed { get; private set; }

        public GameWorld(DeepWoodsGame game, int seed, int width, int height)
        {
            this.game = game;
            Seed = seed;
            rng = new Random(seed);

            // TODO: actual biomes
            biomePairs = TemporaryGetHardcodedBiomes();

            var overground = new SubWorld(rng.Next(), width, height, biomePairs.Select(b => b.Overground).ToList(), SpiralBiomeGenerator, ForestGenerator, GroundTypeGenerator);
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
                        underground = new SubWorld(rng.Next(), biomeRectangle.Width, biomeRectangle.Height, [biomePairs[i].Underground], SingleBiomeGenerator, UndergroundGenerator, GroundTypeGenerator);
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
                subworld.LightManager.FinalGenerate();
            }
        }

        private static List<BiomePair> TemporaryGetHardcodedBiomes()
        {
            return [
                new BiomePair(new TemperateForestBiome(), new GenericBiome(8) { Trees = ["temperate_deciduous_underground_tree1", "temperate_deciduous_underground_tree2"] }),
                new BiomePair(new GenericBiome(1) { Trees = ["tree3", "tree4"] }, new GenericBiome(9) { Trees = ["temperate_conifers_underground_tree1", "temperate_conifers_underground_tree2"] }),
                new BiomePair(new GenericBiome(2) { Trees = ["swamp_tree1", "swamp_tree2"] }, new GenericBiome(10) { Trees = ["swamp_underground_tree1", "swamp_underground_tree2"] }),
                new BiomePair(new GenericBiome(3) { Trees = ["volcanic_tree1", "volcanic_tree2"] }, new GenericBiome(11) { Trees = ["volcanic_underground_tree1", "volcanic_underground_tree2"] }),
                new BiomePair(new GenericBiome(4) { Trees = ["lake_tree1", "lake_tree2"] }, new GenericBiome(12) { Trees = ["lake_underground_tree1", "lake_underground_tree2"] }),
                //new BiomePair(new GenericBiome(5) { Trees = ["lakeunderwater_tree1", "lakeunderwater_tree2"] }, new GenericBiome(12) { Trees = ["tree3", "tree4"] }),
                new BiomePair(new GenericBiome(6) { Trees = ["borealforest_tree1", "borealforest_tree2"] }, new GenericBiome(13) { Trees = ["boreal_underground_tree1", "boreal_underground_tree2"] }),
                new BiomePair(new GenericBiome(7) { Trees = ["magic_tree1", "magic_tree2", "magic_tree1_color2", "magic_tree2_color2"] }, new GenericBiome(14) { Trees = ["magic_underground_tree1", "magic_underground_tree2"] }
                ),
            ];
        }

        public void SwitchToOverground(Player player)
        {
            SwitchSubWorld(player, OverGroundId);
        }

        public void SwitchToUnderground(Player player, int index)
        {
            SwitchSubWorld(player, UnderGroundId(index));
        }

        private void SwitchSubWorld(Player player, string id)
        {
            if (SubWorlds.TryGetValue(id, out SubWorld subWorld))
            {
                playerSubWorlds[player.ID] = subWorld;
            }
        }

        public IBiome GetPlayerBiome(Player player)
        {
            var subworld = GetPlayerSubWorld(player);
            return subworld.Terrain.GetBiome(player.position.RoundToPoint());
        }

        public SubWorld GetPlayerSubWorld(Player player)
        {
            if (playerSubWorlds.TryGetValue(player.ID, out SubWorld subWorld))
            {
                return subWorld;
            }
            return SubWorlds[OverGroundId];
        }

        internal bool IsPlayerOverground(LocalPlayer player)
        {
            return GetPlayerSubWorld(player) == SubWorlds[OverGroundId];
        }

        public bool IsCave(Player player, int currentTileX, int currentTileY)
        {
            var subWorld = GetPlayerSubWorld(player);
            return subWorld.ObjectManager.IsCave(currentTileX, currentTileY);
        }

        public DWObject TryPickUpObject(Player player, int currentTileX, int currentTileY)
        {
            var subWorld = GetPlayerSubWorld(player);
            return subWorld.ObjectManager.TryPickUpObject(currentTileX, currentTileY);
        }

        public Point GetSpawnPosition()
        {
            return SubWorlds[OverGroundId].Terrain.GetSpawnPosition();
        }

        public void Draw(LocalPlayer player)
        {
            var subWorld = GetPlayerSubWorld(player);
            subWorld.Apply(player);

            subWorld.ObjectManager.DrawShadowMap(game.PlayerManager.Players, player);
            subWorld.LightManager.DrawLightMap(player);
            subWorld.Terrain.Draw(player);
            subWorld.ObjectManager.Draw(player);
        }

        public Terrain GetTerrain(Player player)
        {
            var subWorld = GetPlayerSubWorld(player);
            return subWorld.Terrain;
        }

        public void UpdateFogLayer(Player player)
        {
            var subWorld = GetPlayerSubWorld(player);
            subWorld.Terrain.UpdateFogLayer(player);
        }

        public void Update(float deltaTime)
        {
            foreach (var (_, subWorld) in SubWorlds)
            {
                subWorld.Update(deltaTime);
            }
        }

        public void SwitchOverUnderground(Player player, int caveX, int caveY)
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
                    Vector2 newPosition = new Vector2(oppositeCaveX + direction.X, oppositeCaveY + direction.Y);
                    player.SetPosition(newPosition, direction);
                    break;
                }
            }
        }

        public void RemoveFogLayerCheat()
        {
            foreach (var (_, subworld) in SubWorlds)
            {
                subworld.Terrain.RemoveFogLayerCheat();
            }
        }
    }
}
