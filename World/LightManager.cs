using System;
using DeepWoods.Loaders;
using Microsoft.Xna.Framework;

namespace DeepWoods.World
{
    internal class LightManager
    {
        private int numLights = 8;

        private Vector4[] lights = new Vector4[8];
        private Vector2[] lightPositions = new Vector2[8];
        private Vector2[] lightDirection = new Vector2[8];
        private float[] lightSpeed = new float[8];
        private Random rng;
        private int width;
        private int height;

        Vector3 ambientLightColor = new(0.3f, 0.3f, 0.4f);

        public LightManager(int seed, int width, int height)
        {
            rng = new Random(seed);
            this.width = width;
            this.height = height;

            // TODO TEMP light test
            for (int i = 0; i < numLights; i++)
            {
                float distance = 0.5f + rng.NextSingle() * 2f;
                Vector3 color = new Vector3(rng.NextSingle(), rng.NextSingle(), rng.NextSingle());
                Vector2 position = new Vector2(rng.NextSingle() * width, rng.NextSingle() * height);
                Vector2 direction = new Vector2(rng.NextSingle() * 2f - 1f, rng.NextSingle() * 2f - 1f);
                float speed = 0.5f + rng.NextSingle() * 2f;

                lights[i] = new Vector4(color.X, color.Y, color.Z, distance);
                lightPositions[i] = position;
                lightDirection[i] = direction;
                lightSpeed[i] = speed;
            }
        }

        public void MoveLightsForFun(float deltaTime)
        {
            for (int i = 0; i < 8; i++)
            {
                lightPositions[i] += lightDirection[i] * lightSpeed[i] * deltaTime;
                if (lightPositions[i].X < 0)
                {
                    lightPositions[i].X = width;
                }
                else if (lightPositions[i].X > width)
                {
                    lightPositions[i].X = 0;
                }
                if (lightPositions[i].Y < 0)
                {
                    lightPositions[i].Y = height;
                }
                else if (lightPositions[i].Y > height)
                {
                    lightPositions[i].Y = 0;
                }
            }
            EffectLoader.GroundEffect.Parameters["LightPositions"].SetValue(lightPositions);
            EffectLoader.SpriteEffect.Parameters["LightPositions"].SetValue(lightPositions);
        }

        public void Apply()
        {
            EffectLoader.GroundEffect.Parameters["AmbientLightColor"].SetValue(ambientLightColor);
            EffectLoader.GroundEffect.Parameters["Lights"].SetValue(lights);
            EffectLoader.GroundEffect.Parameters["LightPositions"].SetValue(lightPositions);
            EffectLoader.GroundEffect.Parameters["NumLights"].SetValue(numLights);

            EffectLoader.SpriteEffect.Parameters["AmbientLightColor"].SetValue(ambientLightColor);
            EffectLoader.SpriteEffect.Parameters["Lights"].SetValue(lights);
            EffectLoader.SpriteEffect.Parameters["LightPositions"].SetValue(lightPositions);
            EffectLoader.SpriteEffect.Parameters["NumLights"].SetValue(numLights);
        }
    }
}
