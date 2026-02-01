using DeepWoods.Loaders;
using DeepWoods.Main;
using DeepWoods.Players;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DeepWoods.Lighting
{
    public class LightManager
    {
        private readonly List<Light> lights = [];

        private Random rng;
        private int width;
        private int height;

        private static readonly float MaxShadowStrength = 0.5f;

        private InstancedLights instancedLights;

        public Vector3 AmbientColorDay { get; set; } = new(0.8f, 0.8f, 0.8f);
        public Vector3 AmbientColorNight { get; set; } = new(0.1f, 0.1f, 0.2f);
        public Vector3 AmbientColorDusk { get; set; } = new(0.4f, 0.3f, 0.3f);
        public Vector3 AmbientCave { get; set; } = new(0.1f, 0.1f, 0.2f);

        public LightManager(Terrain terrain, int seed)
        {
            rng = new Random(seed);
            width = terrain.Width;
            height = terrain.Height;

            // TODO TEMP light test
            for (int i = 0; i < (width * height) / 100; i++)
            {
                float distance = 0.5f + rng.NextSingle() * 2f;
                Vector3 color = new Vector3(rng.NextSingle(), rng.NextSingle(), rng.NextSingle());
                Vector2 position = new Vector2(rng.NextSingle() * width, rng.NextSingle() * height);
                Vector2 direction = new Vector2(rng.NextSingle() * 2f - 1f, rng.NextSingle() * 2f - 1f);
                float speed = 0.5f + rng.NextSingle() * 2f;

                lights.Add(new Light()
                {
                    color = new Vector4(color.X, color.Y, color.Z, distance),
                    position = position,
                    direction = direction,
                    speed = speed
                });
            }
        }

        public void FinalGenerate()
        {
            if (lights.Count > 0)
            {
                instancedLights = new InstancedLights(rng.Next(), lights);
            }
        }

        public void Update(float deltaTime)
        {
            MoveLightsForFun(deltaTime);
        }

        private void MoveLightsForFun(float deltaTime)
        {
            foreach (var light in lights)
            {
                light.position += light.direction * light.speed * deltaTime;
                if (light.position.X < 0) light.position.X = width;
                if (light.position.X > width) light.position.X = 0;
                if (light.position.Y < 0) light.position.Y = height;
                if (light.position.Y > height) light.position.Y = 0;
            }
            lights[0].position = DeepWoodsMain.Instance.Game.PlayerManager.LocalPlayers[0].position;
            lights[0].color = new(0.6f, 0.6f, 0.6f, DeepWoodsMain.Instance.Game.PlayerManager.LocalPlayers[0].ViewDistance - 1);
            instancedLights?.UpdateLights(lights);
        }

        public void DrawLightMap(LocalPlayer player)
        {
            DeepWoodsMain.Instance.GraphicsDevice.SetRenderTarget(player.LightMap);
            DeepWoodsMain.Instance.GraphicsDevice.Clear(Color.Black);
            DeepWoodsMain.Instance.GraphicsDevice.BlendState = BlendState.Additive;
            DeepWoodsMain.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Matrix view = player.Camera.ShadowView;
            Matrix projection = player.Camera.ShadowProjection;

            EffectLoader.LightEffect.Parameters["ViewProjection"].SetValue(view * projection);

            instancedLights?.Draw();

            DeepWoodsMain.Instance.GraphicsDevice.SetRenderTarget(null);
        }

        public void Apply(LocalPlayer player)
        {
            float shadowSkew;
            float shadowStrength;
            Vector3 ambientLightColor;

            if (DeepWoodsMain.Instance.Game.World.IsPlayerOverground(player))
            {
                double dayDelta = DeepWoodsMain.Instance.Game.Clock.DayDelta;
                if (dayDelta < 0.25)
                {
                    ambientLightColor = Vector3.Lerp(AmbientColorNight, AmbientColorDusk, (float)(dayDelta * 4));
                }
                else if (dayDelta < 0.5)
                {
                    ambientLightColor = Vector3.Lerp(AmbientColorDusk, AmbientColorDay, (float)((dayDelta - 0.25) * 4));
                }
                else if (dayDelta < 0.75)
                {
                    ambientLightColor = Vector3.Lerp(AmbientColorDay, AmbientColorDusk, (float)((dayDelta - 0.5) * 4));
                }
                else
                {
                    ambientLightColor = Vector3.Lerp(AmbientColorDusk, AmbientColorNight, (float)((dayDelta - 0.75) * 4));
                }

                double dayTimeDelta = (Math.Clamp(dayDelta, 0.25, 0.75) - 0.25) * 2.0;
                shadowSkew = (float)(dayTimeDelta * 2.0 - 1.0);
                shadowStrength = MaxShadowStrength * (float)(1.0 - Math.Abs(dayTimeDelta * 2.0 - 1.0));
            }
            else
            {
                ambientLightColor = AmbientCave;
                shadowSkew = 0.5f;
                shadowStrength = MaxShadowStrength;
            }

            EffectLoader.SetParamSafely("ShadowSkew", shadowSkew);
            EffectLoader.SetParamSafely("ShadowStrength", shadowStrength);
            EffectLoader.SetParamSafely("AmbientLightColor", ambientLightColor);
            EffectLoader.SetParamSafely("LightMap", player.LightMap);
        }
    }
}
