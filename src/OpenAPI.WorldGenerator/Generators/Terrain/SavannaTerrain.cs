using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class SavannaTerrain : TerrainBase
    {
        public SavannaTerrain()
        {
            HeightEffect = new GroundEffect(4f);
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return Riverized(generator, generator.Preset.SeaLevel + 3 + HeightEffect.Added(generator, x, y), river);
        }
    }
}