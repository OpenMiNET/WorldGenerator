using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces.Savanna;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Savanna
{
    public class SavannaPlateauBiome : BiomeBase
    {
        public SavannaPlateauBiome()
        {
            Id = 36;
            Name = "Savanna Plateau";
            Temperature = 1.0f;
            Downfall = 0.0f;
            MinHeight = 0.025f;
            MaxHeight = 1.5f;

            Config.AllowRivers = false;
            Config.AllowScenicLakes = false;
            Config.Weight = Weights.Common;

            Terrain = new SavannaPlateauTerrain(true, 10f, 160f, 60f, 40f, 69f, false);
            Surface = new SavannaPlateauSurface(Config, new Grass(), new Dirt());
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#A79D64");
        }
    }
}