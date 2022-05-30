using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils.Noise;

namespace OpenAPI.WorldGenerator.Generators.Surfaces
{
    public class SurfaceBase
    {
        protected int       ShadowStoneBlock;
        protected int       ShadowDesertBlock;
        protected int       TopBlock;
        protected int       FillerBlock;
        protected int       CliffStoneBlock;
        protected int       CliffCobbleBlock;
        protected int IceBlock;
        protected BiomeConfig BiomeConfig;

        protected FastRandom Rnd { get; } = new FastRandom();
        
        protected static readonly int AirId = new Air().Id;
        protected static readonly int StoneId = new Stone().Id;
        protected static readonly int WaterId = new Water().Id;
        
        public SurfaceBase(BiomeConfig config, Block top, Block filler)
        {
            BiomeConfig = config;
            TopBlock = top.GetRuntimeId();
            FillerBlock = filler.GetRuntimeId();
            
            CliffStoneBlock = new Stone().GetRuntimeId();
            CliffCobbleBlock = new Cobblestone().GetRuntimeId();
            ShadowStoneBlock = new Stone().GetRuntimeId();
            ShadowDesertBlock = new Sand().GetRuntimeId();
            IceBlock = new Ice().GetRuntimeId();
        }

        public virtual void PaintTerrain(ChunkColumn column, int blockX, int blockZ, int x, int z, int depth,
            OverworldGeneratorV2 generator, float[] noise, float river, BiomeBase[] biomes)
        {
           // float c = TerrainBase.CalcCliff(x, z, noise);
           // bool cliff = c > 1.4f;
           
            for (int y = 255; y > -1; y--) {
                var b = column.GetBlockId(x, y, z);
              
                if (b == AirId) {
                    depth = -1;
                }
                else if (b == StoneId) {
                    depth++;

                    if (depth == 0 && y > generator.Preset.SeaLevel - 2) {
                        column.SetBlockByRuntimeId(x, y, z, TopBlock);
                    }
                    else if (depth < 4) {
                        column.SetBlockByRuntimeId(x, y, z, FillerBlock);
                    }
                }
                else if (b == WaterId && y >= generator.Preset.SeaLevel)
                {
                    if ((biomes[NoiseMap.GetIndex(x, z)].Type & BiomeType.Snowy) != 0)
                    {
                        column.SetBlockByRuntimeId(x,y,z, IceBlock);
                    }
                }
            }
            
            /*for (int y = 255; y > -1; y--) {
                Block b = column.GetBlockObject(x, y, z);
                if (b is Air) {
                    depth = -1;
                }
                else if (b is Stone) {
                    depth++;

                    if (cliff) {
                        if (depth > -1 && depth < 2) {
                            if (Rnd.Next(3) == 0) {

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
                            column.SetBlock(x, y, z, TopBlock);
                        }
                        else if (depth < 4) {
                            column.SetBlock(x, y, z, FillerBlock);
                        }
                    }
                }
            }*/
        }
    }
}