using System;
using MiNET.Blocks;

namespace OpenAPI.WorldGenerator.Generators.Decorators.Structures
{
	public class LargeJungleTree : TreeStructure
	{
		public override string Name
		{
			get { return "JungleTree"; }
		}


		private int BaseSize = 9;
		private int Roots = 3;
		private float BranchLength = 13f;

		private int Branches = 3;
		private float VerticalStart = 0.32f;
		private float VerticalRand = 0.1f;

		private static readonly int WoodId = new Wood()
		{
			WoodType = "jungle"
		}.GetRuntimeId();
		
		private static readonly int LeavesId = new Leaves()
		{
			OldLeafType = "jungle"
		}.GetRuntimeId();

		public override void Create(StructurePlan plan, int x, int y, int z)
		{
			CheckFloor(plan, x, y - 1, z);
			var baseSize = 9 + Rnd.Next(5);
			if (Roots > 0f)
			{
				for (int k = 0; k < 3; k++)
				{
					var horRootDir = (120 * k) - 40 + Rnd.Next(80);
					var verRootDir = 1.6f + (float) Rnd.NextDouble() * 0.1f;

					GenerateBranch(plan, x, y + Roots, z, horRootDir, verRootDir, Roots * 1.7f, 1f, WoodId);
				}
			}

			for (int i = y + Roots; i < y + baseSize; i++)
			{
				plan.PlaceBlock(x, i, z, WoodId);
			}
			
			float horDir, verDir;
			int eX, eY, eZ;

			for (int j = 0; j < Branches; j++)
			{
				horDir = (120 * j) - 60 + Rnd.Next(120);
				verDir = VerticalStart + (float) Rnd.NextDouble() * VerticalRand;

				eX = x + (int) (Math.Cos(horDir * Math.PI / 180D) * verDir * BranchLength);
				eZ = z + (int) (Math.Sin(horDir * Math.PI / 180D) * verDir * BranchLength);
				eY = y + baseSize + (int) ((1f - verDir) * BranchLength);

				//if (CanGenerateBranch(x, y + BaseSize, z, horDir, verDir, BranchLength, 1f/*, 4f, 1.5f*/) && CanGenerateLeaves(eX, eY, eZ, 4f, 1.5f))
				{
					GenerateBranch(plan, x, y + baseSize, z, horDir, verDir, BranchLength, 1f, WoodId);

					for (int m = 0; m < 1; m++)
					{
						GenerateLeaves(plan, eX, eY, eZ, 4f, 1.5f, LeavesId, WoodId);
					}
				}
			}
		}
	}
}
