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
		private Block mixBlock;

		private static Block[] PlateauBlocks { get; } = new Block[]
		{
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "brown"},
		};
		
		private bool IsModified { get; }
		/// <inheritdoc />
		public SavannaPlateauSurface(BiomeConfig config, Block top, Block filler, bool isModified = false) : base(config, top, filler)
		{
			mixBlock = new Dirt();
			isModified = isModified;
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
            Block b;

            for (int k = 255; k > -1; k--) {
                b = column.GetBlockObject(x, k, z);
                if (b is Air) {
                    depth = -1;
                }
                else if (b is Stone) {
                    depth++;

                    if (cliff)
                    {
	                    if (IsModified)
	                    {
		                    column.SetBlock(x, k, z, PlateauBlocks[k % PlateauBlocks.Length]);
	                    }
	                    else
	                    {
		                    if (depth > -1 && depth < 2)
		                    {
			                    if (Rnd.Next(3) == 0)
			                    {

				                    column.SetBlock(x, k, z, CliffCobbleBlock);
			                    }
			                    else
			                    {
				                    column.SetBlock(x, k, z, CliffStoneBlock);
			                    }
		                    }
		                    else if (depth < 10)
		                    {
			                    column.SetBlock(x, k, z, CliffStoneBlock);
		                    }
	                    }
                    }
                    else {
	                    if (k > 74 + grassRaise) {
                            if (depth == 0) {
                                if (Rnd.Next(5) == 0) {
                                    column.SetBlock(x, k, z, mixBlock);
                                }
                                else {
                                    column.SetBlock(x, k, z, TopBlock);
                                }
                            }
                            else if (depth < 4) {
                                column.SetBlock(x, k, z, FillerBlock);
                            }
                        }
                        else if (depth == 0 && k > 61) {
                            int r = (int) ((k - (62 + grassRaise)) / 2f);
                            if (Rnd.Next(r + 2) == 0) {
                                column.SetBlock(x, k, z, TopBlock);
                            }
                            else if (Rnd.Next((int) (r / 2f) + 2) == 0) {
                                column.SetBlock(x, k, z, mixBlock);
                            }
                            else {
                                column.SetBlock(x, k, z, TopBlock);
                            }
                        }
                        else if (depth < 4) {
                            column.SetBlock(x, k, z, FillerBlock);
                        }
                    }
                }
            }
		}
	}
}