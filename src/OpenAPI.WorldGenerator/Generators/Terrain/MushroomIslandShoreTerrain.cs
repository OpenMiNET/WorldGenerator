namespace OpenAPI.WorldGenerator.Generators.Terrain;

public class MushroomIslandShoreTerrain : TerrainBase
{
	public MushroomIslandShoreTerrain() : base(61.5f)
	{
		
	}
	/// <inheritdoc />
	public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
	{
		return TerrainMarsh(generator, x, y, BaseHeight, river);
	}
}