using System.Numerics;
using MiNET.Blocks;

namespace OpenAPI.WorldGenerator.Generators.Decorators.Structures
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

		private static readonly int WoodId = new Wood()
		{
			WoodType = "oak"
		}.GetRuntimeId();
		
		private static readonly int LeafId = new Leaves()
		{
			OldLeafType = "oak"
		}.GetRuntimeId();
		
		private readonly int _leafRadius = 2;
		public override void Create(StructurePlan plan, int x, int y, int z)
		{
			CheckFloor(plan, x, y - 1, z);
			var location = new Vector3(x, y, z);

			int height = Rnd.Next(4, 5);
			GenerateColumn(plan, location, height, WoodId);
			Vector3 leafLocation = location + new Vector3(0, height, 0);
			GenerateVanillaLeaves(plan, leafLocation, _leafRadius, LeafId);
		}
	}
}
