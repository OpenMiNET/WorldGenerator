namespace OpenAPI.WorldGenerator.Generators.Terrain;

public class IcePlainsTerrain : TerrainBase
{
	public IcePlainsTerrain() : base(65f)
	{
		
	}
        
	public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
	{
		return TerrainPlains(generator, x, y, river, 160f, 10f, 60f,  200f, BaseHeight);
	}
}