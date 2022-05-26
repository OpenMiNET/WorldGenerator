using System.Numerics;
using MiNET.Blocks;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	public class OakTree : TreeStructure
	{
		private bool IsWet { get; }
		public OakTree(bool isWet = false)
		{
			IsWet = isWet;
		}

		public override string Name
		{
			get { return "OakTree"; }
		}

		public override int MaxHeight
		{
			get { return 7; }
		}
		
		private static readonly int WoodId = new Wood()
		{
			WoodType = "oak"
		}.GetRuntimeId();
		
		private static readonly int LeafId = new Leaves()
		{
			OldLeafType = "oak"
		}.GetRuntimeId();
		
		private readonly int _leafRadius = 2;
		public override void Create(ChunkColumn chunk, int x, int y, int z)
		{
			var location = new Vector3(x, y, z);
			if (!ValidLocation(location, _leafRadius)) return;

			int height = Rnd.Next(4, 5);
			GenerateColumn(chunk, location, height, WoodId);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(chunk, leafLocation, _leafRadius, LeafId);
		}
	}
}
