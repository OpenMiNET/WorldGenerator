using System;
using System.Linq;
using System.Numerics;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace OpenAPI.WorldGenerator.Generators.Structures
{
	//Partly taken from https://github.com/Ted80-Minecraft-Mods/Realistic-World-Gen
	//And from https://github.com/SirCmpwn/TrueCraft
	public class TreeStructure : Structure
	{
		private static readonly Block[] ValidBlocks = new Block[]
		{
			new Grass(), new Dirt(), new Farmland()
		};
		
		protected void CheckFloor(StructurePlan plan, int x, int y, int z)
		{
			plan.RequireBlock(x,y,z, ValidBlocks);
		}
		protected void GenerateVanillaLeaves(StructurePlan plan, Vector3 location, int radius, int blockRuntimeId)
		{
			var radiusOffset = radius;
			for (var yOffset = -radius; yOffset <= radius; yOffset = (yOffset + 1))
			{
				var y = location.Y + yOffset;
				if (y > 255)
					continue;
				
				GenerateVanillaCircle(plan, new Vector3(location.X, y, location.Z), radiusOffset, blockRuntimeId);
				if (yOffset != -radius && yOffset % 2 == 0)
					radiusOffset--;
			}
		}

		protected void GenerateVanillaCircle(StructurePlan plan, Vector3 location, int radius, int blockRuntimeId,
			double corner = 0)
		{
			for (var I = -radius; I <= radius; I = (I + 1))
			{
				for (var j = -radius; j <= radius; j = (j + 1))
				{
					var max = (int)Math.Sqrt((I * I) + (j * j));
					if (max <= radius)
					{
						if (I.Equals(-radius) && j.Equals(-radius) || I.Equals(-radius) && j.Equals(radius) ||
							I.Equals(radius) && j.Equals(-radius) || I.Equals(radius) && j.Equals(radius))
						{
							if (corner + radius * 0.2 < 0.4 || corner + radius * 0.2 > 0.7 || corner.Equals(0))
								continue;
						}
						var x = location.X + I;
						var z = location.Z + j;
						if (x < 0 || z > 16) continue;
						if (z < 0 || z > 16) continue;
						
						plan.PlaceBlock((int)x, (int)location.Y, (int)z, blockRuntimeId);
					}
				}
			}
		}

		public static void GenerateColumn(StructurePlan plan, Vector3 location, int height, int blockRuntimeId)
		{
			for (var o = 0; o < height; o++)
			{
				var x = (int)location.X;
				var y = (int)location.Y + o;
				var z = (int)location.Z;
				
				plan.PlaceBlock(x, y, z, blockRuntimeId);
			}
		}

		protected void GenerateCircle(StructurePlan plan, Vector3 location, int radius, int blockRuntimeId)
		{
			for (var I = -radius; I <= radius; I = (I + 1))
			{
				for (var j = -radius; j <= radius; j = (j + 1))
				{
					var max = (int)Math.Sqrt((I * I) + (j * j));
					if (max <= radius)
					{
						var X = location.X + I;
						var Z = location.Z + j;

						if (X < 0 || X >= 16 || Z < 0 || Z >= 256)
							continue;

						var x = (int)X;
						var y = (int)location.Y;
						var z = (int)Z;
						
						plan.PlaceBlock(x, y, z, blockRuntimeId);
					}
				}
			}
		}

		/*protected bool CanGenerateBranch(float x, float y, float z, float horDir, float verDir, float branchLength, float speed/*, float size, float width#1#)
		{
			if (verDir < 0f)
			{
				verDir = -verDir;
			}

			float c = 0f;
			float velY = 1f - verDir;

			if (verDir > 1f)
			{
				verDir = 1f - (verDir - 1f);
			}

			float velX = (float)Math.Cos(horDir * Math.PI / 180D) * verDir;
			float velZ = (float)Math.Sin(horDir * Math.PI / 180D) * verDir;

			while (c < branchLength)
			{
				x += velX;
				y += velY;
				z += velZ;

				c += speed;

				if (x < 0 || x >= 16 || z < 0 || z >= 16) return false;
			}

			/*int i, j, k, s = (int)(size - 1f), w = (int)((size - 1f) * width);
			for (i = -w; i <= w; i++)
			{
				for (j = -s; j <= s; j++)
				{
					for (k = -w; k <= w; k++)
					{
						if (x + i < 0 || x + i >= 16 || z + k < 0 || z + k >= 16) return false;
					}
				}
			}#1#

			return true;
		}*/

		protected void GenerateBranch(StructurePlan plan, float x, float y, float z, double horDir, float verDir, float length, float speed, int blockRuntimeId)
		{
			if (verDir < 0f)
			{
				verDir = -verDir;
			}

			float c = 0f;
			float velY = 1f - verDir;

			if (verDir > 1f)
			{
				verDir = 1f - (verDir - 1f);
			}

			float velX = (float)Math.Cos(horDir * Math.PI / 180D) * verDir;
			float velZ = (float)Math.Sin(horDir * Math.PI / 180D) * verDir;

			while (c < length)
			{
				plan.PlaceBlock((int)x, (int)y, (int)z, blockRuntimeId);
				//chunk.SetBlockByRuntimeId((int)x, (int)y, (int)z, blockRuntimeId);
				
				x += velX;
				y += velY;
				z += velZ;

				c += speed;
			}
		}

		/*protected bool CanGenerateLeaves(int x, int y, int z, float size, float width)
		{
			float dist;
			int i, j, k, s = (int)(size - 1f), w = (int)((size - 1f) * width);
			for (i = -w; i <= w; i++)
			{
				for (j = -s; j <= s; j++)
				{
					for (k = -w; k <= w; k++)
					{
						dist = Math.Abs((float)i / width) + (float)Math.Abs(j) + Math.Abs((float)k / width);
						if (dist <= size - 0.5f || (dist <= size && Rnd.NextDouble() < 0.5))
						{
							var mx = x + i;
							var mz = z + k;

							if (mx < 0 || mx >= 16 || mz < 0 || mz >= 16) return false;
						}
					}
				}
			}
			return true;
		}*/

		protected void GenerateLeaves(StructurePlan plan, int x, int y, int z, float size, float width, int leafBlockRuntimeId, int woodRuntimeId)
		{
			float dist;
			int i, j, k, s = (int)(size - 1f), w = (int)((size - 1f) * width);
			for (i = -w; i <= w; i++)
			{
				for (j = -s; j <= s; j++)
				{
					for (k = -w; k <= w; k++)
					{
						dist = Math.Abs((float)i / width) + (float)Math.Abs(j) + Math.Abs((float)k / width);
						if (dist <= size - 0.5f || (dist <= size && Rnd.NextDouble() < 0.5))
						{ 
							if (dist < 0.6f)
							{
								plan.PlaceBlock(x + i, y + j, z + k, woodRuntimeId);
							}
							
							plan.PlaceBlock(x + i, y + j, z + k, leafBlockRuntimeId);
						}
					}
				}
			}
		}

		/*public bool ValidLocation(Vector3 location, int leafRadius)
		{
			return !(location.X - leafRadius < 0) && !(location.X + leafRadius >= 16) && !(location.Z - leafRadius < 0) &&
				   !(location.Z + leafRadius >= 16);
		}*/

		private static int[] ValidBlockRuntimeIds = new int[]
		{
			new Grass().GetRuntimeId(), new Dirt().GetRuntimeId(), new Farmland().GetRuntimeId()
		};
		/*/// <inheritdoc />
		public override bool CanCreate(ChunkColumn column, int x, int y, int z)
		{
			var block = column.GetBlockObject(x, y, z);

			if (ValidBlockRuntimeIds.Contains(block.GetRuntimeId()))
				return true;

			return false;
			
			return base.CanCreate(column, x, y, z);
		}*/
	}
}
