
namespace DeepWoods.Objects
{
    public class DWObjectDefinition
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Standing { get; set; }
        public bool Glowing { get; set; }
        public int AnimationFrames { get; set; } = 0;
        public int AnimationFrameOffset { get; set; } = 0;
        public int AnimationFPS { get; set; } = 0;
    }
}
