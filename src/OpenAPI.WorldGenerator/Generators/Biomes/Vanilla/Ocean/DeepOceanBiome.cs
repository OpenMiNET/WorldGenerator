using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean
{
    public class DeepOceanBiome : BiomeBase
    {
        public DeepOceanBiome()
        {
            Id = 24;
            Name = "Deep Ocean";
            Temperature = 0.5f;
            Downfall = 0.5f;
            MinHeight = -1.8F;
            MaxHeight = 0.1f;
            Terrain = new DeepOceanTerrain();
            
            
            Type = BiomeType.Ocean;
            Config.Weight = Weights.Common;
            
            Config.AllowScenicLakes = false;
            Config.AllowRivers = false;
            
            Color = ColorUtils.FromHtml("#000030");
        }
    }
}