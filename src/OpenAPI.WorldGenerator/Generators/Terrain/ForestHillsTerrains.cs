namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class ForestHillsTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainHighland(generator, x, y, river, 10f, 68f, 30f, BaseHeight /*- generator.Preset.SeaLevel*/);
        }
    }
}