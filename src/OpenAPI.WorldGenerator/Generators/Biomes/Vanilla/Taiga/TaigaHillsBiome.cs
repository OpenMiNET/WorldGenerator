using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Surfaces.Taiga;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Taiga
{
    public class TaigaHillsBiome : BiomeBase
    {
        public TaigaHillsBiome()
        {
            Id = 19;
            Name = "Taiga Hills";
            Temperature = 0.2f;
            Downfall = 0.7f;
            MinHeight = 0.2f;
            MaxHeight = 0.7f;
            
            Config.AllowRivers = false;
            Config.AllowScenicLakes = false;
            
            Terrain = new TaigaHillsTerrain();
            Surface = new TaigaSurface(Config, new Grass(), new Dirt());
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#163933");
        }
    }
}