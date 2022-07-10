using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class PlainsTerrain : TerrainBase
    {
        public PlainsTerrain() : base(65f)
        {
            HeightEffect = new GroundEffect(4f);
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return Riverized(generator, BaseHeight + HeightEffect.Added(generator, x, y), river);
           
            return TerrainPlains(generator, x, y, river, 160f, 10f, 60f,  generator.Preset.HeightScale, BaseHeight);
        }
    }
}