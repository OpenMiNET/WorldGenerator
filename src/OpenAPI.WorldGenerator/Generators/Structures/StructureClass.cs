using System;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	public class Structure
	{
		protected readonly Random Rnd = new Random();
		public virtual string Name
		{
			get { return null; }
		}

		public virtual int MaxHeight { get { return 0; } }

		public virtual void Create(ChunkColumn column, int x, int y, int z)
		{
		
		}

		public virtual bool CanCreate(ChunkColumn column, int x, int y, int z)
		{
			return true;
		}

		protected Block GetBlockObject(int runtimeId)
		{
			BlockStateContainer blockStateContainer = BlockFactory.BlockPalette[runtimeId];
			Block blockById = BlockFactory.GetBlockById(blockStateContainer.Id);
			blockById.SetState(blockStateContainer.States);
			blockById.Metadata = (byte) blockStateContainer.Data;
			return blockById;
		}
	}
}
