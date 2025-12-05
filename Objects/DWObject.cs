using Microsoft.Xna.Framework;

namespace DeepWoods.Objects
{
    public class DWObject
    {
        public DWObjectDefinition Def { get; private set; }

        public Vector2 WorldPos { get; private set; }

        public Rectangle TexRect => new(Def.X, Def.Y, Def.Width, Def.Height);

        public DWObject(Vector2 pos, DWObjectDefinition def)
        {
            WorldPos = pos;
            Def = def;
        }
    }
}
