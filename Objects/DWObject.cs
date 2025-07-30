using Microsoft.Xna.Framework;

namespace DeepWoods.Objects
{
    internal class DWObject
    {
        public DWObjectDefinition Def { get; private set; }

        public Vector2 WorldPos { get; private set; }

        public Rectangle TexRect => new(Def.X, Def.Y, Def.Width, Def.Height);

        public bool IsStanding => Def.Standing;
        public bool IsGlowing => Def.Glowing;
        public int AnimationFrames => Def.AnimationFrames;
        public int AnimationFrameOffset => Def.AnimationFrameOffset;
        public int AnimationFPS => Def.AnimationFPS;

        public DWObject(Vector2 pos, DWObjectDefinition def)
        {
            WorldPos = pos;
            Def = def;
        }
    }
}
