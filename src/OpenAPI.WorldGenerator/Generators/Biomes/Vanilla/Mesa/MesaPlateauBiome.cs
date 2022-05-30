using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Surfaces.Mesa;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa
{
    public class MesaPlateauBiome : BiomeBase
    {
        public MesaPlateauBiome()
        {
            Id = 38;
            Name = "Wooded Badlands Plateau";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 0.025f;
            MaxHeight = 1.5f;

            Config.AllowRivers = false;
            Config.AllowScenicLakes = false;
            Config.Weight = Weights.Uncommon;
            
            Terrain = new VanillaMesaPlateauTerrain(true, 35f, 160f, 60f, 40f, 69f);
            Surface = new MesaSurface(Config, new Sand() {SandType = "red"}, new RedSandstone());
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#B09765");
        }
    }
}