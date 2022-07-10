using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces.Mushroom;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mushroom
{
    public class MushroomIslandShoreBiome : BiomeBase
    {
        public MushroomIslandShoreBiome()
        {
            Id = 15;
            Name = "Mushroom Fields Shore";
            Temperature = 0.9f;
            Downfall = 1.0f;
            MinHeight = -1f;
            MaxHeight = 0.1f;
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#A000FF");
            Config.AllowScenicLakes = false;
            Config.Weight = Weights.SuperRare;
            
            Surface = new MushroomSurface(Config, new Mycelium(), new Stone(), 0f);
            Terrain = new MushroomIslandShoreTerrain();
        }
    }
}