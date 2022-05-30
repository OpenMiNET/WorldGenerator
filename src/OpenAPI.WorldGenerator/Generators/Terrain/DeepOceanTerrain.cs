namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class DeepOceanTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainOcean(generator, x, y, river, 40f);
        }
    }
}