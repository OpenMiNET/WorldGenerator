using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces.Plains;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Plains
{
    public class IcePlainsBiome : BiomeBase
    {
        public IcePlainsBiome()
        {
            Id = 12;
            Name = "Snowy Tundra";
            Temperature = 0.0f;
            Downfall = 0.5f;
            MinHeight = 0.125f;
            MaxHeight = 0.5f;

            Terrain = new IcePlainsTerrain();
            Surface = new IcePlainsSurface(Config, new Snow(), new Dirt(), new Snow(), new Dirt());
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#FFFFFF");
        }
    }
}