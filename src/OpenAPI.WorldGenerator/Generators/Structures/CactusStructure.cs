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
		public override void Create(ChunkColumn chunk, int x, int y, int z)
		{
			//if (blocks[OverworldGenerator.GetIndex(x, y - 1, z)] != 12) return; //Not sand, do not generate.

			var growth = Rnd.Next(0x1, 0x15);
			for (int modifiedY = y; modifiedY < y + _height; modifiedY++)
			{
				if (!CheckSafe(chunk, x, modifiedY, z)) break;

				//blocks[OverworldGenerator.GetIndex(x, modifiedY, z)] = 81;
				//metadata[OverworldGenerator.GetIndex(x, modifiedY, z)] = (byte) growth;
				chunk.SetBlockByRuntimeId(x, modifiedY, z, CactusBlockId); //Cactus block
				//chunk.SetMetadata(x, modifiedY, z, (byte)growth);
			}
		}

		private bool CheckSafe(ChunkColumn chunk, int x, int y, int z)
		{
			if (!chunk.IsAir(x - 1, y, z)) return false;
			if (!chunk.IsAir(x + 1, y, z)) return false;
			if (!chunk.IsAir(x, y, z - 1)) return false;
			if (!chunk.IsAir(x, y, z + 1)) return false;
			
			return true;
		}

		private bool CheckSafe(Level level, int x, int y, int z)
		{
			if (level.IsAir(new BlockCoordinates(x - 1, y, z))) return false;
			if (level.IsAir(new BlockCoordinates(x + 1, y, z))) return false;
			if (level.IsAir(new BlockCoordinates(x, y, z - 1))) return false;
			if (level.IsAir(new BlockCoordinates(x, y, z + 1))) return false;

			return true;
		}
	}
}
