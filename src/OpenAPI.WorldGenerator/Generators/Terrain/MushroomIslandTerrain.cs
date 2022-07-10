namespace OpenAPI.WorldGenerator.Generators.Terrain;

public class MushroomIslandTerrain : TerrainBase
{
	/// <inheritdoc />
	public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
	{
		return TerrainGrasslandFlats(generator, x, y, river, 40f, BaseHeight);
	}
}