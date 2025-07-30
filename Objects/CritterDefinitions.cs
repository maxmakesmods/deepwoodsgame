
namespace DeepWoods.Objects
{
    internal class CritterDefinitions
    {
        public enum Critter
        {
            CROW,
            HEDGEHOG,
            BEEHIVE,
            FROG
        }

        public static DWObjectDefinition GetCritterDefinition(Critter critter)
        {
            return new DWObjectDefinition()
            {
                Name = critter.ToString(),
                X = 32 * (int)critter,
                Y = 0,
                Width = 32,
                Height = 32,
                Standing = true,
                Glowing = false,
                AnimationFrames = 8,
                AnimationFrameOffset = 32,
                AnimationFPS = 4,
            };
        }
    }
}
