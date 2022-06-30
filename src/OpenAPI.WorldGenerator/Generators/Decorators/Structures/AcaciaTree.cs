using System.Numerics;
using MiNET.Blocks;

namespace OpenAPI.WorldGenerator.Generators.Decorators.Structures
{
    public class AcaciaTree : TreeStructure
    {
        public override string Name
        {
            get { return "AcaciaTree"; }
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
	    public override void Create(StructurePlan plan, int x, int y, int z)
	    {
		    var location = new Vector3(x, y, z);
		    CheckFloor(plan, x, y - 1, z);
	//	    if (!ValidLocation(location, _leafRadius)) return;

		    int height = Rnd.Next(5, 6);
		    
		    GenerateColumn(plan, location, height, WoodId);
		    GenerateVanillaLeaves(plan, location + new Vector3(0, height, 0), _leafRadius, LeafId);
	    }
    }
}
