using OpenAPI.WorldGenerator.Generators.Biomes.Config;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils;

namespace OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Forest;

public class FlowerForestBiome : BiomeBase
{
	public FlowerForestBiome()
	{
		Id = 132; //132?
		Name = "Flower Forest";
		Temperature = 0.7f;
		Downfall = 0.8f;
		MinHeight = 0.1f; //TODO
		MaxHeight = 0.2f;
		Terrain = new ForestTerrain();
            
		Config.Weight = Weights.Uncommon;
		
		Color = ColorUtils.FromHtml("#2D8E49");
	}
}