using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator
{
    public static class Extensions
    {
        public static void SetBlock(this ChunkColumn column, int x, int y, int z, int blockId)
        {
            column.SetBlock(x, y, z, BlockFactory.GetBlockById(blockId));
        }

        public static bool IsAir(this ChunkColumn column, int x, int y, int z)
        {
            return column.GetBlockId(x, y, z) == 0;
        }
    }
}