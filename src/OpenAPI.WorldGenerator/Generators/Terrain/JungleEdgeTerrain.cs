using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class JungleEdgeTerrain : TerrainBase
    {
        private GroundEffect GroundEffect { get; set; } = new GroundEffect(4f);
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return Riverized(generator, generator.Preset.SeaLevel + 3 + GroundEffect.Added(generator, x, y), river);
        }
    }
}