using System.Numerics;
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
        
        private static readonly int WoodId = new Wood()
        {
	        WoodType = "acacia"
        }.GetRuntimeId();
		
        private static readonly int LeafId = new Leaves2()
        {
	        NewLeafType = "acacia"
        }.GetRuntimeId();
        
        private readonly int _leafRadius = 2;
	    public override void Create(ChunkColumn column, int x, int y, int z)
	    {
		    var location = new Vector3(x, y, z);
		    if (!ValidLocation(location, _leafRadius)) return;

		    int height = Rnd.Next(5, 6);
		    
		    GenerateColumn(column, location, height, WoodId);
		    GenerateVanillaLeaves(column, location + new Vector3(0, height, 0), _leafRadius, LeafId);
	    }
    }
}
