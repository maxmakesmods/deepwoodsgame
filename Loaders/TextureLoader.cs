
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeepWoods.Loaders
{
    internal static class TextureLoader
    {
        public static Texture2D GroundTilesTexture { get; private set; }
        public static Texture2D BluenoiseTexture { get; private set; }
        public static Texture2D TreeTexture { get; private set; }
        public static Texture2D TowerTexture { get; private set; }
        public static Texture2D WagonTexture { get; private set; }
        public static Texture2D CampfireAbandonedTexture { get; private set; }

        public static void Load(ContentManager content)
        {
            GroundTilesTexture = content.Load<Texture2D>("textures/groundtiles");
            BluenoiseTexture = content.Load<Texture2D>("textures/bluenoise_rgba");
            TreeTexture = content.Load<Texture2D>("objects/tree");
            TowerTexture = content.Load<Texture2D>("objects/tower");
            WagonTexture = content.Load<Texture2D>("objects/abandoned_wagon");
            CampfireAbandonedTexture = content.Load<Texture2D>("objects/campfire_abandoned");
        }
    }
}
