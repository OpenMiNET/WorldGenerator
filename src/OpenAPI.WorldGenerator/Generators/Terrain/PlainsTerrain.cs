using OpenAPI.WorldGenerator.Generators.Effects;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class PlainsTerrain : TerrainBase
    {
        private GroundEffect _groundEffect = new GroundEffect(4f);
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            //return Riverized(65f + _groundEffect.Added(world, x, y), river);
           
            return TerrainPlains(x, y, generator, river, 160f, 10f, 60f, 200f, generator.Preset.SeaLevel + 4);
        }
    }
}