using System.Numerics;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
    class BirchTree : TreeStructure
	{
        public override string Name
        {
            get { return "BirchTree"; }
        }

        public override int MaxHeight
        {
            get { return 7; }
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
		
		public override void Create(ChunkColumn column, int x, int y, int z)
	    {
		//	var block = blocks[OverworldGenerator.GetIndex(x, y - 1, z)];
		//	if (block != 2 && block != 3) return;

			var location = new Vector3(x, y, z);
			if (!ValidLocation(location, LeafRadius)) return;

			int height = Rnd.Next(4, MaxHeight);
			GenerateColumn(column, location, height, WoodId);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(column, leafLocation, LeafRadius, LeafId);
		}
	}
}
