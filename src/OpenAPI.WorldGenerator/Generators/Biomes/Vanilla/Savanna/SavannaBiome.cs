using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces.Savanna;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Savanna
{
    public class SavannaBiome : BiomeBase
    {
        public SavannaBiome()
        {
            Id = 35;
            Name = "Savanna";
            Temperature = 1.2f;
            Downfall = 0.0f;
            MinHeight = 0.005f;
            MaxHeight = 0.125f;
            
            Terrain = new SavannaTerrain();
            Surface = new SavannaSurface(Config, new Grass(), new Dirt(), new Grass(), 13f, 0.27f);

            Config.Weight = Weights.Common;
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#BDB25F");
        }
    }
}