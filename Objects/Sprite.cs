using Microsoft.Xna.Framework;

namespace DeepWoods.Objects
{
    internal class Sprite
    {
        public Vector2 WorldPos { get; private set; }
        public Rectangle TexRect { get; private set; }
        public bool IsStanding { get; private set; }
        public bool IsGlowing { get; private set; }

        public int AnimationFrames { get; private set; }
        public int AnimationFrameOffset { get; private set; }
        public int AnimationFPS { get; private set; }

        public Sprite(Vector2 pos, Rectangle texRect, bool isStanding, bool isGlowing = false, int animationFrames = 0, int animationFrameOffset = 0, int animationFPS = 0)
        {
            WorldPos = pos;
            IsStanding = isStanding;
            IsGlowing = isGlowing;
            TexRect = texRect;
            AnimationFrames = animationFrames;
            AnimationFrameOffset = animationFrameOffset;
            AnimationFPS = animationFPS;
        }
    }
}
