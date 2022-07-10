using System;
using System.Collections.Generic;
using System.Linq;
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

		public virtual void Create(StructurePlan plan, int x, int y, int z)
		{
			
		}
	}

	public class StructurePlan
	{
		private LinkedList<StructureAction> Actions { get; }

		public StructurePlan()
		{
			Actions = new LinkedList<StructureAction>();
		}

		public void AddAction(StructureAction action)
		{
			Actions.AddLast(action);
		}

		public void PlaceBlock(int x, int y, int z, int runtimeId)
		{
			AddAction(new PlaceBlockAction(x, y, z, runtimeId));
		}

		public void RequireBlock(int x, int y, int z, params Block[] validBlocks)
		{
			AddAction(new RequireBlockAction(x, y, z, validBlocks));
		}

		public bool TryExecute(ChunkColumn column)
		{
			foreach (var action in Actions)
			{
				if (!action.Validate(column))
					return false;
			}

			foreach (var action in Actions)
			{
				action.Execute(column);
			}

			return true;
		}

		public class RequireBlockAction : StructureAction
		{
			public int X { get; }
			public int Y { get; }
			public int Z { get; }
			public Type[] RuntimeIds { get; }

			public RequireBlockAction(int x, int y, int z, params Block[] runtimeIds)
			{
				X = x;
				Y = y;
				Z = z;
				RuntimeIds = runtimeIds.Select(x => x.GetType()).ToArray();
			}
				
			/// <inheritdoc />
			public override bool Validate(ChunkColumn column)
			{
				var blockAtPosition = column.GetBlockObject(X, Y, Z);
				var blockType = blockAtPosition.GetType();

				return RuntimeIds.Any(x => x.IsAssignableFrom(blockType));
			}

			/// <inheritdoc />
			public override void Execute(ChunkColumn column)
			{
				
			}
		}

		public class PlaceBlockAction : StructureAction
		{
			public int X { get; }
			public int Y { get; }
			public int Z { get; }
			public int RuntimeId { get; }

			public PlaceBlockAction(int x, int y, int z, int runtimeId)
			{
				X = x;
				Y = y;
				Z = z;
				RuntimeId = runtimeId;
			}
			
			/// <inheritdoc />
			public override bool Validate(ChunkColumn column)
			{
				return X >= 0 && X <= 15 && Z >= 0 && Z <= 15;
			}

			/// <inheritdoc />
			public override void Execute(ChunkColumn column)
			{
				column.SetBlockByRuntimeId(X, Y, Z, RuntimeId);
			}
		}
		
		public abstract class StructureAction
		{
			public abstract bool Validate(ChunkColumn column);
			public abstract void Execute(ChunkColumn column);
		}
	}
}
