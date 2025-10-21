
using DeepWoods.Game;
using DeepWoods.Objects;
using DeepWoods.World.Biomes;
using DeepWoods.World.Generators;
using System;
using System.Collections.Generic;

namespace DeepWoods.World
{
    internal class SubWorld
    {
        private readonly Random rng;

        public Terrain Terrain { get; private set; }
        public ObjectManager ObjectManager { get; private set; }
        public LightManager LightManager { get; private set; }


        public SubWorld(AllTheThings att, int seed, int width, int height, List<IBiome> biomes, Generator biomeGenerator, Generator forestGenerator, Generator groundTypeGenerator)
        {
            rng = new Random(seed);
            Terrain = new Terrain(att, rng.Next(), width, height, biomes, biomeGenerator, forestGenerator, groundTypeGenerator);
            ObjectManager = new ObjectManager(att, Terrain, rng.Next());
            LightManager = new LightManager(att, Terrain, rng.Next());
        }

        internal void Update(double dayDelta, float deltaTime)
        {
            LightManager.Update(dayDelta, deltaTime);
        }

        internal void Apply()
        {
            Terrain.Apply();
            LightManager.Apply();
        }
    }
}
