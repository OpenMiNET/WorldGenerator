namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class ForestTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
          //  return TerrainForest(x,y, generator, river, ge)
            GroundNoise = GetGroundNoise(x, y, GroundVariation, generator);

            float m = Hills(x, y, 10f, generator);

            float floNoise = generator.Preset.SeaLevel + 3 + GroundNoise + m;

            return Riverized(generator, floNoise, river);
        }
    }
}