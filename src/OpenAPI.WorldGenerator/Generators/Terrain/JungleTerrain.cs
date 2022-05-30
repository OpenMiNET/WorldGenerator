namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class JungleTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainFlatLakes(generator, x, y, river, 66f);
        }
    }
}