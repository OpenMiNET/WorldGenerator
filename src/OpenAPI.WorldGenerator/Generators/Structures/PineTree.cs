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

		public override int MaxHeight
		{
			get { return 10; }
		}

		private static readonly int WoodId = new Wood()
		{
			WoodType = "oak"
		}.GetRuntimeId();
		
		private static readonly int LeafId = new Leaves()
		{
			OldLeafType = "oak"
		}.GetRuntimeId();
		
		public override void Create(ChunkColumn column, int x, int y, int z)
		{
			//var block = blocks[OverworldGenerator.GetIndex(x, y - 1, z)];
			//if (block != 2 && block != 3) return;

			var location = new Vector3(x, y, z);
			if (!ValidLocation(location, _leafRadius)) return;

			var height = Rnd.Next(7, 8);
			GenerateColumn(column, location, height, WoodId);
			for (var Y = 1; Y < height; Y++)
			{
				if (Y % 2 == 0)
				{
					GenerateVanillaCircle(column, location + new Vector3(0, Y + 1, 0), _leafRadius - 1, LeafId, 1);
					continue;
				}
				GenerateVanillaCircle(column, location + new Vector3(0, Y + 1, 0), _leafRadius, LeafId, 1);
			}

			GenerateTopper(column, location + new Vector3(0, height, 0), 0x1);
		}

		protected void GenerateTopper(ChunkColumn chunk, Vector3 location, byte type = 0x0)
		{
			var sectionRadius = 1;
			GenerateCircle(chunk, location, sectionRadius, LeafId);
			var top = location + new Vector3(0, 1, 0);
			var x = (int)location.X;
			var y = (int)location.Y + 1;
			var z = (int)location.Z;

			chunk.SetBlockByRuntimeId(x, y, z, LeafId);
			//chunk.SetMetadata(x, y, z, 1);
			
			if (type == 0x1 && y < 256)
				GenerateVanillaCircle(chunk, new Vector3(x, y, z), sectionRadius, LeafId);
		}
	}
}
