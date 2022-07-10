using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	public class CactusStructure : Structure
	{
		public CactusStructure() : this(3)
		{

		}

		private int _height;

		public CactusStructure(int height)
		{
			_height = height;
		}

		private static readonly int CactusBlockId = new Cactus().GetRuntimeId();
		public override void Create(StructurePlan plan, int x, int y, int z)
		{
			plan.RequireBlock(x,y - 1,z, new Sand());
			//if (blocks[OverworldGenerator.GetIndex(x, y - 1, z)] != 12) return; //Not sand, do not generate.

			//var growth = Rnd.Next(0x1, 0x15);
			for (int modifiedY = y; modifiedY < y + _height; modifiedY++)
			{
				//if (!CheckSafe(plan, x, modifiedY, z)) break;

				//blocks[OverworldGenerator.GetIndex(x, modifiedY, z)] = 81;
				//metadata[OverworldGenerator.GetIndex(x, modifiedY, z)] = (byte) growth;
				plan.PlaceBlock(x, modifiedY, z, CactusBlockId); //Cactus block
				//chunk.SetMetadata(x, modifiedY, z, (byte)growth);
			}
		}
	}
}
