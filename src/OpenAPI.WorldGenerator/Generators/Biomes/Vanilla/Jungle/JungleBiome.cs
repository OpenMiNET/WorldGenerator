using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces.Jungle;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Jungle
{
    public class JungleBiome : BiomeBase
    {
        public JungleBiome()
        {
            Id = 21;
            Name = "Jungle";
            Temperature = 1.2f;
            Downfall = 0.9f;
            MinHeight = 0.1f;
            MaxHeight = 0.4f;

            Terrain = new JungleTerrain();
            Surface = new JungleSurface(Config, new Grass(), new Dirt(), 0f, 1.5f, 60f, 65f, 1.5f, new Podzol(), 0.09f);
            Config.Weight = Weights.Common;
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#537B09");
            
            // Config.WeightMultiplier = 1.f;
        }
    }
}