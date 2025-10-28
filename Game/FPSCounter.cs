using Microsoft.Xna.Framework;

namespace DeepWoods.Game
{
    public class FPSCounter
    {
        public int FPS { get; private set; } = 0;
        public int SPF { get; private set; } = 0;

        private int counter;
        private double frameTimeSum;

        public void CountFrame(GameTime gameTime)
        {
            frameTimeSum += gameTime.ElapsedGameTime.TotalSeconds;
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
