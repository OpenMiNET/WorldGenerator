using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class PlainsTerrain : TerrainBase
    {
        private GroundEffect _groundEffect = new GroundEffect(4f);
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return Riverized(generator, 65f + _groundEffect.Added(generator, x, y), river);
           
            return TerrainPlains(generator, x, y, river, 160f, 10f, 60f,  generator.Preset.HeightScale, BaseHeight);
        }
    }
    
    public class IcePlainsTerrain : TerrainBase
    {
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainPlains(generator, x, y, river, 160f, 10f, 60f,  generator.Preset.HeightScale, BaseHeight);
        }
    }
}