using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean
{
    public class FrozenOceanBiome : BiomeBase
    {
        public FrozenOceanBiome()
        {
            Id = 10;
            Name = "Frozen Ocean";
            Temperature = 0.0f;
            Downfall = 0.5f;
            MinHeight = -1f;
            MaxHeight = 0.5f;
            Terrain = new OceanTerrain();
            
            Type = BiomeType.Ocean | BiomeType.Snowy;
            Config.Weight = Weights.Common;
            
            Config.AllowScenicLakes = false;
            Config.AllowRivers = false;
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#7070D6");
        }
    }
}