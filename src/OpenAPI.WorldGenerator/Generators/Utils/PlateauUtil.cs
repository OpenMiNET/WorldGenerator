using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace OpenAPI.WorldGenerator.Generators.Utils
{
    public class PlateauUtil
    {
        //public static Block GetPlateauBand(OverworldGeneratorV2 world, BiomeBase rBiome, int x, int y, int z) {
        //    return GetBand(rBiome, y);
       // }
        
        public static float StepIncrease(float simplexVal, float start, float finish, float height) {
            return (simplexVal <= start) ? 0 : (simplexVal >= finish) ? height : ((simplexVal - start) / (finish - start)) * height;
        }
    }
}