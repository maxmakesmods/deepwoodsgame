
namespace DeepWoods.World.Biomes
{
    internal class BiomePair
    {
        public IBiome Overground { get; set; }
        public IBiome Underground { get; set; }

        public BiomePair(IBiome overground, IBiome underground)
        {
            Overground = overground;
            Underground = underground;
        }
    }
}
