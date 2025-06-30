using Microsoft.Xna.Framework;

namespace DeepWoods.Objects
{
    internal class Sprite
    {
        public Vector2 WorldPos { get; private set; }
        public Rectangle TexRect { get; private set; }
        public bool IsStanding { get; private set; }
        public bool IsGlowing { get; private set; }

        public Sprite(Vector2 pos, Rectangle texRect, bool isStanding, bool isGlowing)
        {
            WorldPos = pos;
            IsStanding = isStanding;
            IsGlowing = isGlowing;
            TexRect = texRect;
        }
    }
}
