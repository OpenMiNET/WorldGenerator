using System.Numerics;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	public class PineTree : TreeStructure
	{
		private readonly int _leafRadius = 2;

		public override string Name
		{
			get { return "PineTree"; }
		}

		private static readonly int WoodId = new Wood()
		{
			WoodType = "oak"
		}.GetRuntimeId();
		
		private static readonly int LeafId = new Leaves()
		{
			OldLeafType = "oak"
		}.GetRuntimeId();
		
		public override void Create(StructurePlan plan, int x, int y, int z)
		{
			CheckFloor(plan, x, y - 1, z);
			var location = new Vector3(x, y, z);

			var height = Rnd.Next(7, 8);
			GenerateColumn(plan, location, height, WoodId);
			for (var Y = 1; Y < height; Y++)
			{
				if (Y % 2 == 0)
				{
					GenerateVanillaCircle(plan, location + new Vector3(0, Y + 1, 0), _leafRadius - 1, LeafId, 1);
					continue;
				}
				GenerateVanillaCircle(plan, location + new Vector3(0, Y + 1, 0), _leafRadius, LeafId, 1);
			}

			GenerateTopper(plan, location + new Vector3(0, height, 0), 0x1);
		}

		protected void GenerateTopper(StructurePlan plan, Vector3 location, byte type = 0x0)
		{
			var sectionRadius = 1;
			GenerateCircle(plan, location, sectionRadius, LeafId);
			var top = location + new Vector3(0, 1, 0);
			var x = (int)location.X;
			var y = (int)location.Y + 1;
			var z = (int)location.Z;

			plan.PlaceBlock(x, y, z, LeafId);
			//chunk.SetMetadata(x, y, z, 1);
			
			if (type == 0x1 && y < 256)
				GenerateVanillaCircle(plan, new Vector3(x, y, z), sectionRadius, LeafId);
		}
	}
}
