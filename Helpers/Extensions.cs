
using Microsoft.Xna.Framework;

namespace DeepWoods.Helpers
{
    internal static class Extensions
    {
        public static Vector3 CenterV3(this Rectangle rect, float z = 0f)
        {
            return new Vector3(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f, z);
        }

        public static Vector4 GetBoundsV4(this Rectangle rect)
        {
            return new Vector4(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }

        public static Vector2 GetSizeV2(this Rectangle rect)
        {
            return new Vector2(rect.Width, rect.Height);
        }

        public static bool IsBitFlagSet(this byte value, byte flag)
        {
            return (value & flag) != 0;
        }

        public static bool IsBitFlagSet(this ushort value, ushort flag)
        {
            return (value & flag) != 0;
        }

        public static bool IsBitFlagSet(this uint value, uint flag)
        {
            return (value & flag) != 0;
        }

        public static bool IsBitFlagSet(this ulong value, ulong flag)
        {
            return (value & flag) != 0;
        }
    }
}
