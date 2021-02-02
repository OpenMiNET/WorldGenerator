using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Surfaces.Taiga
{
	public class TaigaSurface : SurfaceBase
	{
		private Block MixBlock;
		/// <inheritdoc />
		public TaigaSurface(BiomeConfig config, Block top, Block filler) : base(config, top, filler)
		{
			MixBlock = new Podzol();
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
            var simplex = generator.SimplexInstance(0);
            float p = simplex.GetValue(blockX / 8f, blockZ / 8f) * 0.5f;
            float c = TerrainBase.CalcCliff(x, z, noise);
            int cliff = 0;

            Block b;
            for (int y = 255; y > -1; y--) {
                b = column.GetBlockObject(x, y, z);
                if (b is Air) {
                    depth = -1;
                }
                else if (b is Stone) {
                    depth++;

                    if (depth == 0) {

                        if (c > 0.45f && c > 1.5f - ((y - 60f) / 65f) + p) {
                            cliff = 1;
                        }
                        if (c > 1.5f) {
                            cliff = 2;
                        }
                        if (y > 110 + (p * 4) && c < 0.3f + ((y - 100f) / 50f) + p) {
                            cliff = 3;
                        }

                        if (cliff == 1) {
                            if (Rnd.Next(3) == 0) {

                                column.SetBlock(x, y, z, CliffCobbleBlock);
                            }
                            else {

                                column.SetBlock(x, y, z, CliffStoneBlock);
                            }
                        }
                        else if (cliff == 2) {
                            column.SetBlock(x, y, z, ShadowStoneBlock);
                        }
                        else if (cliff == 3) {
                            column.SetBlock(x, y, z, new Snow());
                        }
                        else if (simplex.GetValue(blockX / 50f, blockZ / 50f) + p * 0.6f > 0.24f) {
                            column.SetBlock(x, y, z, MixBlock);
                        }
                        else {
                            column.SetBlock(x, y, z, new Grass());
                        }
                    }
                    else if (depth < 6) {
                        if (cliff == 1) {
                            column.SetBlock(x, y, z, CliffStoneBlock);
                        }
                        else if (cliff == 2) {
                            column.SetBlock(x, y, z, ShadowStoneBlock);
                        }
                        else if (cliff == 3) {
                            column.SetBlock(x, y, z, new Snow());
                        }
                        else {
                            column.SetBlock(x, y, z, new Dirt());
                        }
                    }
                }
            }
		}
	}
}