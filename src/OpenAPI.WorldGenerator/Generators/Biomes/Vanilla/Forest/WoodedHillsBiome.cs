using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
    public class WoodedHillsBiome : BiomeBase
    {
        public WoodedHillsBiome()
        {
            Id = 18;
            Name = "Wooded Hills";
            Temperature = 0.7f;
            Downfall = 0.8f;
            MinHeight = 0.2f;
            MaxHeight = 0.6f;
            
            Config.AllowRivers = false;
            Config.AllowScenicLakes = false;
            
            Terrain = new ForestHillsTerrain();
            
            Config.Weight = Weights.Common;
            Color = ColorUtils.FromHtml("#22551C");
        }
    }
}