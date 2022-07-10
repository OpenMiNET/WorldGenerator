namespace OpenAPI.WorldGenerator.Generators.Terrain
{
public class VanillaMesaPlateauTerrain : TerrainBase
{
	private readonly float[] _height;
	private readonly int _heightLength;
	private readonly float _strength;

	public VanillaMesaPlateauTerrain(bool riverGen,
		float heightStrength,
		float canyonWidth,
		float canyonHeight,
		float canyonStrength,
		float baseHeight) : base(baseHeight)
	{
		_height = new float[] {32.0f, 0.4f};
		_strength = 10f;
		_heightLength = _height.Length;
	}

	/// <inheritdoc />
	public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
	{
		return TerrainPlateau(generator, x, y, river, _height, border, _strength, _heightLength, 100f, false);
	}
}
}