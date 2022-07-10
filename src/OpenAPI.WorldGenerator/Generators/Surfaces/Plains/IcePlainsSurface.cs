using MiNET.Blocks;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Surfaces.Plains;

public class IcePlainsSurface : SurfaceBase
{
    private int CliffBlock1 { get; }
    private int CliffBlock2 { get; }
	/// <inheritdoc />
	public IcePlainsSurface(BiomeConfig config, Block top, Block filler, Block cliff1, Block cliff2) : base(config, top, filler)
    {
        CliffBlock1 = cliff1.GetRuntimeId();
        CliffBlock2 = cliff2.GetRuntimeId();
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
        bool cliff = c > 1.4f;

        int b;

        for (int y = 255; y > -1; y--)
        {
            b = column.GetBlockId(x, y, z);

            if (b == AirId)
            {
                depth = -1;
            }
            else if (b == StoneId)
            {
                depth++;

                if (cliff)
                {
                    if (depth > -1 && depth < 2)
                    {
                        column.SetBlockByRuntimeId(x, y, z, CliffBlock1);
                    }
                    else if (depth < 10)
                    {
                        column.SetBlockByRuntimeId(x, y, z, CliffBlock2);
                    }
                }
                else
                {
                    if (depth == 0 && y > 61)
                    {
                        column.SetBlockByRuntimeId(x, y, z, TopBlock);
                    }
                    else if (depth < 4)
                    {
                        column.SetBlockByRuntimeId(x, y, z, FillerBlock);
                    }
                }
            }
        }
    }
}