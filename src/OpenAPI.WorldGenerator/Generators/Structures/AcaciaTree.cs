using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
    public class AcaciaTree : TreeStructure
    {
        public override string Name
        {
            get { return "AcaciaTree"; }
        }

        public override int MaxHeight
        {
            get { return 8; }
        }

	    public override void Create(ChunkColumn column, int x, int y, int z)
	    {
		    if (x > 11 || z > 11) return;
		    if (x < 5 || z < 5) return;

		//    var block = blocks[OverworldGenerator.GetIndex(x, y - 1, z)];
		//	if (block != 2 && block != 3) return;

			base.Create(column, x, y, z);
	    }
    }
}
