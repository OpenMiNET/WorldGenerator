using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Surfaces.Jungle
{
	public class JungleSurface : SurfaceBase
	{
		private float min;

		private float sCliff    = 1.5f;
		private float sHeight   = 60f;
		private float sStrength = 65f;
		private float cCliff    = 1.5f;

		private Block mixBlock;
		private float       mixHeight;
		
		/// <inheritdoc />
		public JungleSurface(BiomeConfig config, Block top, Block filler, float minCliff, float stoneCliff,
			float stoneHeight, float stoneStrength, float clayCliff, Block mix, float mixSize) : base(config, top, filler)
		{
			min = minCliff;

			sCliff = stoneCliff;
			sHeight = stoneHeight;
			sStrength = stoneStrength;
			cCliff = clayCliff;

			mixBlock = mix;// this.getConfigBlock(config.SURFACE_MIX_BLOCK.get(), mix);

			mixHeight = mixSize;
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
            float c = TerrainBase.CalcCliff(x, z, noise);
            int cliff = 0;
            bool m = false;

            Block b;

            for (int k = 255; k > -1; k--)
            {
	            b = column.GetBlockObject(x,k,z);

	            if (b is Air)
	            {
		            depth = -1;
	            }
	            else if (b is Stone)
	            {
		            depth++;

		            if (depth == 0)
		            {

			            float p = simplex.GetValue(blockX / 8f, blockZ / 8f, k / 8f) * 0.5f;

			            if (c > min && c > sCliff - ((k - sHeight) / sStrength) + p)
			            {
				            cliff = 1;
			            }

			            if (c > cCliff)
			            {
				            cliff = 2;
			            }

			            if (cliff == 1)
			            {
				            if (Rnd.Next(3) == 0)
				            {
					            column.SetBlock(x,k,z, CliffCobbleBlock);
				            }
				            else
				            {

					            column.SetBlock(x, k, z, CliffStoneBlock);
				            }
			            }
			            else if (cliff == 2)
			            {
				            column.SetBlock(x, k, z, ShadowStoneBlock);
			            }
			            else if (k < 63)
			            {
				            if (k < 62)
				            {
					            column.SetBlock(x, k, z, FillerBlock);
				            }
				            else
				            {
					            column.SetBlock(x, k, z,  TopBlock);
				            }
			            }
			            else if (simplex.GetValue(blockX / 12f, blockZ / 12f) > mixHeight)
			            {
				            column.SetBlock(x, k, z, mixBlock);
				            m = true;
			            }
			            else
			            {
				            column.SetBlock(x, k, z, TopBlock);
			            }
		            }
		            else if (depth < 6)
		            {
			            if (cliff == 1)
			            {
				            column.SetBlock(x, k, z, CliffStoneBlock);
			            }
			            else if (cliff == 2)
			            {
				            column.SetBlock(x, k, z, ShadowStoneBlock);
			            }
			            else
			            {
				            column.SetBlock(x, k, z, FillerBlock);
			            }
		            }
	            }
            }
		}
	}
}