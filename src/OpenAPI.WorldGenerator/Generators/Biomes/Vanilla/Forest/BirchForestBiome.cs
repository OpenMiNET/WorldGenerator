using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
    public class BirchForestBiome : BiomeBase
    {
        public BirchForestBiome()
        {
            Id = 27;
            Name = "Birch Forest";
            Temperature = 0.6f;
            Downfall = 0.6f;
            MinHeight = 0.1f;
            MaxHeight = 0.2f;
            Terrain = new BirchForestTerrain();
            Config.Weight = Weights.Common - 1;

            Color = ColorUtils.FromHtml("#307444");
        }
    }
}