using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
public class BirchForestHillsMBiome : BiomeBase
{
	public BirchForestHillsMBiome()
	{
		Id = 156;
		Name = "Birch Forest Hills M";
		Temperature = 0.6f;
		Downfall = 0.6f;
		MinHeight = 0.35f;
		MaxHeight = 0.45f;
		
		Config.AllowRivers = false;
		Config.AllowScenicLakes = false;
		
		Terrain = new BirchForestHillsMTerrain();
		Config.Weight = Weights.Rare;
		
		Color = ColorUtils.FromHtml("#47875A");
	}
}
}