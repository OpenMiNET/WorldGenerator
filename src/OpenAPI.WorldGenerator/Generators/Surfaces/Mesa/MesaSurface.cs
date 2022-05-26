using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Generators.Utils;

namespace OpenAPI.WorldGenerator.Generators.Surfaces.Mesa
{
	public class MesaSurface : SurfaceBase
	{
		private int   grassRaise = 0;
		private int mixBlock;
		private int mix2Block;

		private static int[] PlateauBlocks { get; } = new int[]
		{
			new StainedHardenedClay(){Color = "yellow"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "yellow"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "yellow"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new StainedHardenedClay(){Color = "orange"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "red"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "red"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new StainedHardenedClay(){Color = "orange"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(), 
			new HardenedClay().GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new StainedHardenedClay(){Color = "orange"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "silver"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "white"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new StainedHardenedClay(){Color = "brown"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new StainedHardenedClay(){Color = "orange"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(), 
			new StainedHardenedClay(){Color = "orange"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new StainedHardenedClay(){Color = "orange"}.GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
			new HardenedClay().GetRuntimeId(),
		};
		
		/// <inheritdoc />
		public MesaSurface(BiomeConfig config, Block top, Block filler) : base(config, top, filler)
		{
			mixBlock = new StainedHardenedClay()
			{
				Color = "orange"
			}.GetRuntimeId();

			mix2Block = new RedSandstone().GetRuntimeId();
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

                    if (cliff) {
                        column.SetBlockByRuntimeId(x, k, z, PlateauBlocks[k % PlateauBlocks.Length]);
                    }
                    else {

                        if (k > 74 + grassRaise) {
                            if (depth == 0) {
                                if (Rnd.Next(5) == 0) {
                                    column.SetBlockByRuntimeId(x, k, z, mix2Block);
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
                                column.SetBlockByRuntimeId(x, k, z, mixBlock);
                            }
                            else if (Rnd.Next((int) (r / 2f) + 2) == 0) {
                                column.SetBlockByRuntimeId(x, k, z, mix2Block);
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