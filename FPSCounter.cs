
namespace DeepWoods
{
    internal class FPSCounter
    {
        public int FPS { get; private set; } = 0;

        private int counter;
        private float frameTimeSum;

        public void CountFrame(float frameTime)
        {
            frameTimeSum += frameTime;
            counter++;
            if (frameTimeSum > 1)
            {
                FPS = (int)(counter / frameTimeSum);
                frameTimeSum = 0;
                counter = 0;
            }
        }
    }
}
