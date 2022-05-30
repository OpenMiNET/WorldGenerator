using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle
{
    public class JungleHillsBiome : BiomeBase
    {
        public JungleHillsBiome()
        {
            Id = 22;
            Name = "Jungle Hills";
            Temperature = 1.2f;
            Downfall = 0.9f;
            MinHeight = 0.2f;
            MaxHeight = 1.8f;
            
            Config.AllowRivers = false;
            Config.AllowScenicLakes = false;
            
            Config.Weight = Weights.Uncommon;
            Config.Weight = Weights.Common;
            Config.TreeDensity = 0.4f;
            
            Terrain = new JungleHillsTerrain(72f, 40f);
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#2C4205");
        }
    }
}