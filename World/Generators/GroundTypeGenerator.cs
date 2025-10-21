
namespace DeepWoods.World.Generators
{
    internal class GroundTypeGenerator : Generator
    {
        protected override void GenerateImpl()
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
