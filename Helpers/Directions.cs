using Microsoft.Xna.Framework;
using System;

namespace DeepWoods.Helpers
{
    internal class Directions
    {
        public enum Dir
        {
            TOP_LEFT = 0,
            LEFT = 1,
            BOTTOM_LEFT = 2,
            BOTTOM = 3,
            BOTTOM_RIGHT = 4,
            RIGHT = 5,
            TOP_RIGHT = 6,
            TOP = 7,
        }

        public static readonly Point[] Points = [
            new Point(-1, 1),
            new Point(-1, 0),
            new Point(-1, -1),
            new Point(0, -1),
            new Point(1, -1),
            new Point(1, 0),
            new Point(1, 1),
            new Point(0, 1),
        ];

        public static readonly Point[] TwoAxisPoints = [
            new Point(-1, 0),
            new Point(-1, 0),
            new Point(-1, 0),
            new Point(0, -1),
            new Point(1, 0),
            new Point(1, 0),
            new Point(1, 0),
            new Point(0, 1),
        ];

        public static Dir GetDirFromVector(Vector2 v)
        {
            if (v.X < 0)
            {
                if (v.Y < 0)
                {
                    return Dir.BOTTOM_LEFT;
                }
                else if (v.Y > 0)
                {
                    return Dir.TOP_LEFT;
                }
                else
                {
                    return Dir.LEFT;
                }
            }
            else if (v.X > 0)
            {
                if (v.Y < 0)
                {
                    return Dir.BOTTOM_RIGHT;
                }
                else if (v.Y > 0)
                {
                    return Dir.TOP_RIGHT;
                }
                else
                {
                    return Dir.RIGHT;
                }
            }
            else if (v.Y < 0)
            {
                return Dir.BOTTOM;
            }
            else if (v.Y > 0)
            {
                return Dir.TOP;
            }
            else
            {
                return Dir.BOTTOM;
            }
        }

        internal static Point GetPointFromVector(Vector2 v)
        {
            return Points[(int)GetDirFromVector(v)];
        }

        internal static Point GetTwoAxisPointFromVector(Vector2 v)
        {
            return TwoAxisPoints[(int)GetDirFromVector(v)];
        }
    }
}
