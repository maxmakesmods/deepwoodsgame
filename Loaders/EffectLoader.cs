
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
    }
}
