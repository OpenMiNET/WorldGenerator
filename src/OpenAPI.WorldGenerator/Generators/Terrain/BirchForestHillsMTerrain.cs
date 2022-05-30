namespace OpenAPI.WorldGenerator.Generators.Terrain;

public class BirchForestHillsMTerrain : TerrainBase
{
	private float HillStrength { get; set; } = 70f;
        
	public BirchForestHillsMTerrain() {

	}

	public BirchForestHillsMTerrain(float baseHeight, float hillStrength) : base(baseHeight) 
	{
		HillStrength = hillStrength;
	}
        
	public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
	{
		return TerrainHighland(generator, x, y, river, 10f, 68f, HillStrength, BaseHeight - generator.Preset.SeaLevel);
	}
}