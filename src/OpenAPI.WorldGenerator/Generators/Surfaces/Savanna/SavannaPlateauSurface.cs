using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Surfaces.Savanna
{
	public class SavannaPlateauSurface : SurfaceBase
	{
		private int   grassRaise = 0;
		private int mixBlock;

		private static int[] PlateauBlocks { get; } = new int[]
		{
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
		};
		
		private bool IsModified { get; }
		/// <inheritdoc />
		public SavannaPlateauSurface(BiomeConfig config, Block top, Block filler, bool isModified = false) : base(config, top, filler)
		{
			mixBlock = new Dirt().GetRuntimeId();
			IsModified = isModified;
		}

		/// <inheritdoc />
		public override void PaintTerrain(ChunkColumn column,
			int blockX,
			int blockZ,
			int x,
			int z,
			int depth,
			OverworldGeneratorV2 generator,
			float[] noise,
			float river,
			BiomeBase[] biomes)
		{
			float c = TerrainBase.CalcCliff(x, z, noise);
            bool cliff = c > 1.3f;
            int b;

            for (int k = 255; k > -1; k--) {
                b = column.GetBlockId(x, k, z);
                if (b == AirId) {
                    depth = -1;
                }
                else if (b == StoneId) {
                    depth++;

                    if (cliff)
                    {
	                    if (IsModified)
	                    {
		                    column.SetBlockByRuntimeId(x, k, z, PlateauBlocks[k % PlateauBlocks.Length]);
	                    }
	                    else
	                    {
		                    if (depth > -1 && depth < 2)
		                    {
			                    if (Rnd.Next(3) == 0)
			                    {

				                    column.SetBlockByRuntimeId(x, k, z, CliffCobbleBlock);
			                    }
			                    else
			                    {
				                    column.SetBlockByRuntimeId(x, k, z, CliffStoneBlock);
			                    }
		                    }
		                    else if (depth < 10)
		                    {
			                    column.SetBlockByRuntimeId(x, k, z, CliffStoneBlock);
		                    }
	                    }
                    }
                    else {
	                    if (k > 74 + grassRaise) {
                            if (depth == 0) {
                                if (Rnd.Next(5) == 0) {
                                    column.SetBlockByRuntimeId(x, k, z, mixBlock);
                                }
                                else {
                                    column.SetBlockByRuntimeId(x, k, z, TopBlock);
                                }
                            }
                            else if (depth < 4) {
                                column.SetBlockByRuntimeId(x, k, z, FillerBlock);
                            }
                        }
                        else if (depth == 0 && k > 61) {
                            int r = (int) ((k - (62 + grassRaise)) / 2f);
                            if (Rnd.Next(r + 2) == 0) {
                                column.SetBlockByRuntimeId(x, k, z, TopBlock);
                            }
                            else if (Rnd.Next((int) (r / 2f) + 2) == 0) {
                                column.SetBlockByRuntimeId(x, k, z, mixBlock);
                            }
                            else {
                                column.SetBlockByRuntimeId(x, k, z, TopBlock);
                            }
                        }
                        else if (depth < 4) {
                            column.SetBlockByRuntimeId(x, k, z, FillerBlock);
                        }
                    }
                }
            }
		}
	}
}