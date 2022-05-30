using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class MesaTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainMesa(generator, x, y, river, border);// Riverized(generator, BaseHeight + GroundEffect.Added(generator, x, y), river);
        }
    }
}