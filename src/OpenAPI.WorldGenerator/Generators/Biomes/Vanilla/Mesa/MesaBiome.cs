using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Surfaces.Mesa;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa
{
    public class MesaBiome : BiomeBase
    {
        public MesaBiome()
        {
            Id = 37;
            Name = "Badlands";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 0.1f;
            MaxHeight = 0.2f;
            
            Config.Weight = Weights.Uncommon;
            
            Terrain = new MesaTerrain();
            Surface = new MesaSurface(Config, new Sand() {SandType = "red"}, new RedSandstone());
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#D94515");
        }   
    }
}