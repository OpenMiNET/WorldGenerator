namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mushroom
{
    public class MushroomIslandBiome : BiomeBase
    {
        public MushroomIslandBiome()
        {
            Id = 14;
            Name = "Mushroom Fields";
            Temperature = 0.9f;
            Downfall = 1.0f;
            MinHeight = 0.2f;
            MaxHeight = 1f;
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#FF00FF");
        }
    }
}