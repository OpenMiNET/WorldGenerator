namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class ForestTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            GroundNoise = GetGroundNoise(generator, x, y, GroundVariation);
            return TerrainForest(generator, x, y, river, generator.Preset.SeaLevel + 3 + GroundNoise);
        }
    }
}