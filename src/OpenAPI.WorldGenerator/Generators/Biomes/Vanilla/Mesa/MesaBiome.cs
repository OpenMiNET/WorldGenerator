using MiNET.Blocks;
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
            Name = "Mesa";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 0.1f;
            MaxHeight = 0.2f;
            
            Config.WeightMultiplier = 1.25f;
            
            Terrain = new MesaTerrain();
            Surface = new MesaSurface(Config, new Sand() {SandType = "red"}, new RedSandstone());
        }   
    }
}