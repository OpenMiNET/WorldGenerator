using MiNET.Blocks;
using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Surfaces;
using OpenAPI.WorldGenerator.Generators.Surfaces.Mesa;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Mesa
{
    public class MesaPlateauFBiome : BiomeBase
    {
        public MesaPlateauFBiome()
        {
            Id = 39;
            Name = "Badlands Plateau";
            Temperature = 2.0f;
            Downfall = 0.0f;
            MinHeight = 0.25f;
            MaxHeight = 1.5f;//0.25f;

            Config.AllowRivers = false;
            Config.AllowScenicLakes = false;
            Config.Weight = Weights.Uncommon;
          //  SurfaceBlock = 12; //Surface = Red Sand
          //  SurfaceMetadata = 1;

         //   SoilBlock = 179; //Soil = Red Sandstone
           // Terrain = new MesaPlateauTerrain(67);
           Terrain = new VanillaMesaPlateauTerrain(true, 35f, 160f, 60f, 40f, 69f);
            Surface = new MesaSurface(Config, new Sand()
            {
                SandType = "red"
            }, new RedSandstone());
            
            Color = OpenAPI.WorldGenerator.Utils.ColorUtils.FromHtml("#CA8C65");
        }
    }
}