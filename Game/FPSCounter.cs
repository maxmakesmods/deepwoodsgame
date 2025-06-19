namespace DeepWoods.Game
{
    internal class FPSCounter
    {
        public int FPS { get; private set; } = 0;
        public int SPF { get; private set; } = 0;

        private int counter;
        private double frameTimeSum;

        public void CountFrame(double frameTime)
        {
            frameTimeSum += frameTime;
            counter++;
            if (frameTimeSum > 1)
            {
                FPS = (int)(counter / frameTimeSum);
                SPF = (int)(1000 * frameTimeSum / counter);
                frameTimeSum = 0;
                counter = 0;
            }
        }
    }
}
