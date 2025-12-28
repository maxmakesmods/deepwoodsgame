
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.Loaders
{
    internal static class EffectLoader
    {
        public static Effect GroundEffect { get; private set; }
        public static Effect SpriteEffect { get; private set; }

        public static void Load(ContentManager content)
        {
            GroundEffect = content.Load<Effect>("effects/GroundEffect");
            SpriteEffect = content.Load<Effect>("effects/SpriteEffect");
        }

        public static void SetParamSafely(string name, Matrix value)
        {
            GroundEffect.Parameters[name]?.SetValue(value);
            SpriteEffect.Parameters[name]?.SetValue(value);
        }

        public static void SetParamSafely(string name, Texture value)
        {
            GroundEffect.Parameters[name]?.SetValue(value);
            SpriteEffect.Parameters[name]?.SetValue(value);
        }

        public static void SetParamSafely(string name, Vector4 value)
        {
            GroundEffect.Parameters[name]?.SetValue(value);
            SpriteEffect.Parameters[name]?.SetValue(value);
        }

        public static void SetParamSafely(string name, Vector2 value)
        {
            GroundEffect.Parameters[name]?.SetValue(value);
            SpriteEffect.Parameters[name]?.SetValue(value);
        }

        public static void SetParamSafely(string name, float value)
        {
            GroundEffect.Parameters[name]?.SetValue(value);
            SpriteEffect.Parameters[name]?.SetValue(value);
        }

        public static void SetParamSafely(string name, int value)
        {
            GroundEffect.Parameters[name]?.SetValue(value);
            SpriteEffect.Parameters[name]?.SetValue(value);
        }
    }
}
