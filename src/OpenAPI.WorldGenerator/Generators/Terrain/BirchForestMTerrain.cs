using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
public class BirchForestMTerrain : TerrainBase
{
	private GroundEffect GroundEffect { get; set; }= new GroundEffect(4f);
        
	public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
	{
		return TerrainPlains(generator, x, y, river, 160f, 10f, 60f, 80f, 65f);
		return TerrainForest(generator, x, y, river, generator.Preset.SeaLevel + 3 + GroundEffect.Added(generator, x, y));
	}
}
}