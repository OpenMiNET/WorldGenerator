using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Swamp
{
    public class SwamplandBiome : BiomeBase
    {
        public SwamplandBiome()
        {
            Id = 6;
            Name = "Swamp";
            Temperature = 0.8f;
            Downfall = 0.5f;
            MinHeight = -0.2f;
            MaxHeight = 0.1f;

            Terrain = new SwamplandTerrain();
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#07F9B2");
        }    
    }
}