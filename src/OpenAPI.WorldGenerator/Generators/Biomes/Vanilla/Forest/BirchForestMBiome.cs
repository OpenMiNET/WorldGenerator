using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest
{
public class BirchForestMBiome : BiomeBase
{
	public BirchForestMBiome()
	{
		Id = 155;
		Name = "Birch Forest M";
		Temperature = 0.6f;
		Downfall = 0.6f;
		MinHeight = 0.35f;
		MaxHeight = 0.45f;
		Terrain = new BirchForestMTerrain();
		Config.Weight = Weights.Uncommon;
		
		Color = ColorUtils.FromHtml("#589C6C");
	}
}
}