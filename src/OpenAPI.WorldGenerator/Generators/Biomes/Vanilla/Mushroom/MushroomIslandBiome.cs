using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces.Mushroom;
using OpenAPI.WorldGenerator.Generators.Terrain;

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
            Config.AllowScenicLakes = false;
            Config.Weight = Weights.SuperRare;

            Surface = new MushroomSurface(Config, new Mycelium(), new Stone(), 0f);
            Terrain = new MushroomIslandTerrain();
        }
    }
}