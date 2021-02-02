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
		private Block mixBlock;
		private Block mix2Block;

		private static Block[] PlateauBlocks { get; } = new Block[]
		{
			new StainedHardenedClay(){Color = "yellow"},
			new StainedHardenedClay(){Color = "yellow"},
			new StainedHardenedClay(){Color = "yellow"},
			new HardenedClay(),
			new HardenedClay(),
			new StainedHardenedClay(){Color = "orange"},
			new StainedHardenedClay(){Color = "red"},
			new StainedHardenedClay(){Color = "red"},
			new HardenedClay(),
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "white"},
			new HardenedClay(),
			new HardenedClay(),
			new HardenedClay(),
			new StainedHardenedClay(){Color = "orange"},
			new HardenedClay(), 
			new HardenedClay(),
			new HardenedClay(),
			new StainedHardenedClay(){Color = "orange"},
			new StainedHardenedClay(){Color = "silver"},
			new StainedHardenedClay(){Color = "white"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "brown"},
			new StainedHardenedClay(){Color = "brown"},
			new HardenedClay(),
			new StainedHardenedClay(){Color = "orange"},
			new HardenedClay(), 
			new StainedHardenedClay(){Color = "orange"},
			new HardenedClay(),
			new StainedHardenedClay(){Color = "orange"},
			new HardenedClay(),
			new HardenedClay(),
			new HardenedClay(),
		};
		
		/// <inheritdoc />
		public MesaSurface(BiomeConfig config, Block top, Block filler) : base(config, top, filler)
		{
			mixBlock = new StainedHardenedClay()
			{
				Color = "orange"
			};

			mix2Block = new RedSandstone();
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

                    if (cliff) {
                        column.SetBlock(x, k, z, PlateauBlocks[k % PlateauBlocks.Length]);
                    }
                    else {

                        if (k > 74 + grassRaise) {
                            if (depth == 0) {
                                if (Rnd.Next(5) == 0) {
                                    column.SetBlock(x, k, z, mix2Block);
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
                                column.SetBlock(x, k, z, mixBlock);
                            }
                            else if (Rnd.Next((int) (r / 2f) + 2) == 0) {
                                column.SetBlock(x, k, z, mix2Block);
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