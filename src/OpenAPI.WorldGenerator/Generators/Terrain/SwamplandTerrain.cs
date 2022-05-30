namespace OpenAPI.WorldGenerator.Generators.Terrain
{
	public class SwamplandTerrain : TerrainBase
	{
		/// <inheritdoc />
		public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
		{
			return TerrainMarsh(generator, x, y, generator.Preset.SeaLevel - .5f, river);
		}
	}
}