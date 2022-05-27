using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces.Savanna;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla
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
            Config.WeightMultiplier = 1.05f;

            Terrain = new SavannaPlateauTerrain(true, 35f, 160f, 60f, 40f, 69f, true);
            Surface = new SavannaPlateauSurface(Config, new Grass(), new Dirt());
        }
    }
}