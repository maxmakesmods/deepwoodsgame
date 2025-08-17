
namespace DeepWoods.World.Generators
{
    internal class GroundTypeGenerator : Generator
    {
        public GroundTypeGenerator(Tile[,] tiles)
            : base(tiles)
        {
        }

        public override void Generate()
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (tiles[x, y].isOpen)
                    {
                        tiles[x, y].groundType = tiles[x, y].biome.OpenGroundType;
                    }
                    else
                    {
                        tiles[x, y].groundType = tiles[x, y].biome.ClosedGroundType;
                    }
                }
            }
        }
    }
}
