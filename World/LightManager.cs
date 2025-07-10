using System;
using DeepWoods.Loaders;
using Microsoft.Xna.Framework;

namespace DeepWoods.World
{
    internal class LightManager
    {
        private int numLights = 0;

        private Vector4[] lights = new Vector4[8];
        private Vector2[] lightPositions = new Vector2[8];
        private Vector2[] lightDirection = new Vector2[8];
        private float[] lightSpeed = new float[8];
        private Random rng;
        private int width;
        private int height;

        private static readonly Vector3 AMBIENT_DAY = new(0.7f, 0.7f, 0.7f);
        private static readonly Vector3 AMBIENT_NIGHT = new(0.3f, 0.3f, 0.4f);
        private static readonly Vector3 AMBIENT_DUSK = new(0.5f, 0.4f, 0.4f);

        private static readonly float MaxShadowStrength = 0.5f;

        Vector3 ambientLightColor = AMBIENT_DAY;

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

        public void Update(double dayDelta, float deltaTime)
        {
            if (dayDelta < 0.25)
            {
                ambientLightColor = Vector3.Lerp(AMBIENT_NIGHT, AMBIENT_DUSK, (float)(dayDelta * 4));
            }
            else if (dayDelta < 0.5)
            {
                ambientLightColor = Vector3.Lerp(AMBIENT_DUSK, AMBIENT_DAY, (float)((dayDelta - 0.25) * 4));
            }
            else if (dayDelta < 0.75)
            {
                ambientLightColor = Vector3.Lerp(AMBIENT_DAY, AMBIENT_DUSK, (float)((dayDelta - 0.5) * 4));
            }
            else
            {
                ambientLightColor = Vector3.Lerp(AMBIENT_DUSK, AMBIENT_NIGHT, (float)((dayDelta - 0.75) * 4));
            }

            double dayTimeDelta = (Math.Clamp(dayDelta, 0.25, 0.75) - 0.25) * 2.0;

            float shadowSkew = (float)(dayTimeDelta * 2.0 - 1.0);

            float shadowStrength = MaxShadowStrength * (float)(1.0 - Math.Abs(dayTimeDelta * 2.0 - 1.0));

            EffectLoader.SpriteEffect.Parameters["ShadowSkew"].SetValue(shadowSkew);
            //EffectLoader.SpriteEffect.Parameters["ShadowStrength"].SetValue(shadowStrength);

            EffectLoader.GroundEffect.Parameters["ShadowStrength"].SetValue(shadowStrength);

            MoveLightsForFun(deltaTime);
        }

        private void MoveLightsForFun(float deltaTime)
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
