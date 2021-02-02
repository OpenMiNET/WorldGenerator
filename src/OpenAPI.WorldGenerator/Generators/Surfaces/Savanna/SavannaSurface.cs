using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Surfaces.Savanna
{
	public class SavannaSurface : SurfaceBase
	{
		private Block mixBlock;
		private float       width;
		private float       height;
		
		/// <inheritdoc />
		public SavannaSurface(BiomeConfig config, Block top, Block filler, Block mix, float mixWidth, float mixHeight) : base(config, top, filler)
		{
			mixBlock = mix;

			width = mixWidth;
			height = mixHeight;
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
			
			FastRandom       rand    = new FastRandom();
			var simplex = generator.SimplexInstance(0);
			float        c       = TerrainBase.CalcCliff(x, z, noise);
			bool      cliff   = c > 1.4f;

			for (int y = 255; y > -1; y--) {
				Block b = column.GetBlockObject(x, y, z);
				if (b is Air) {
					depth = -1;
				}
				else if (b is Stone) {
					depth++;

					if (cliff) {
						if (depth > -1 && depth < 2) {
							if (rand.Next(3) == 0) {
								column.SetBlock(x, y, z, CliffCobbleBlock);
							}
							else {

								column.SetBlock(x, y, z, CliffStoneBlock);
							}
						}
						else if (depth < 10) {
							column.SetBlock(x, y, z, CliffStoneBlock);
						}
					}
					else {
						if (depth == 0 && y > 61) {
							if (simplex.GetValue(blockX / width, blockZ / width) > height) // > 0.27f, i / 12f
							{
								column.SetBlock(x, y, z, mixBlock);
							}
							else {
								column.SetBlock(x, y, z, TopBlock);
							}
						}
						else if (depth < 4) {
							column.SetBlock(x, y, z, FillerBlock);
						}
					}
				}
			}
		}
	}
}