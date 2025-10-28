
namespace DeepWoods.World
{
    public class InGameClock
    {
        private static readonly double DefaultTimeScale = 60.0;

        private double accumulatedTime = 0.0;

        public double TimeScale { get; set; } = 1.0;

        public void Update(double deltaTime)
        {
            accumulatedTime += deltaTime * DefaultTimeScale * TimeScale;
        }

        public void SetTime(int day, int hour, int minute)
        {
            accumulatedTime = day * 86400.0 + hour * 3600.0 + minute * 60.0;
        }

        public double DayDelta => (accumulatedTime / 86400.0) % 1.0;

        public int Day => (int)(accumulatedTime / 86400.0);
        public int Hour => (int)(accumulatedTime / 3600.0) % 24;
        public int Minute => (int)(accumulatedTime / 60.0) % 60;
    }
}
