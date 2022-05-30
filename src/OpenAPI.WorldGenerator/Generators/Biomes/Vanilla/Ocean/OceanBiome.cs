using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean
{
    public class OceanBiome : BiomeBase
    {
        public OceanBiome()
        {
            Id = 0;
            Name = "Ocean";
            Temperature = 0.5f;
            Downfall = 0.5f;
            MinHeight = -1f;
            MaxHeight = 0.5f;
            Terrain = new OceanTerrain();

            Type = BiomeType.Ocean;
            Config.Weight = Weights.Common;
            
            Config.AllowScenicLakes = false;
            Config.AllowRivers = false;
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#000070");
        }
    }
}