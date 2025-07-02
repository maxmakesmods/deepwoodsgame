
using Microsoft.Xna.Framework;

namespace DeepWoods.Helpers
{
    internal static class Extensions
    {
        public static Vector3 CenterV3(this Rectangle rect, float z = 0f)
        {
            return new Vector3(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, z);
        }

        public static Vector4 GetBoundsV4(this Rectangle rect)
        {
            return new Vector4(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }

        public static Vector2 GetSizeV2(this Rectangle rect)
        {
            return new Vector2(rect.Width, rect.Height);
        }
    }
}
