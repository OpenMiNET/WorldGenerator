using System.Numerics;
using MiNET.Blocks;

namespace OpenAPI.WorldGenerator.Generators.Decorators.Structures
{
    class BirchTree : TreeStructure
	{
        public override string Name
        {
            get { return "BirchTree"; }
        }

		private const int LeafRadius = 2;
		private static readonly int WoodId = new Wood()
		{
			WoodType = "birch"
		}.GetRuntimeId();
		
		private static readonly int LeafId = new Leaves()
		{
			OldLeafType = "birch"
		}.GetRuntimeId();
		
		public override void Create(StructurePlan plan, int x, int y, int z)
	    {
		//	var block = blocks[OverworldGenerator.GetIndex(x, y - 1, z)];
		//	if (block != 2 && block != 3) return;
			CheckFloor(plan, x, y - 1, z);
			var location = new Vector3(x, y, z);

			int height = Rnd.Next(4, 7);
			GenerateColumn(plan, location, height, WoodId);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(plan, leafLocation, LeafRadius, LeafId);
		}
	}
}
