namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class DesertHillsTerrain : TerrainBase
    {
        private float _hillStart;
        private float _landHeight;
        private float _hillWidth;

        public DesertHillsTerrain(float hillStart, float landHeight, float baseHeight, float hillWidth) : base(baseHeight) {

            _hillStart = hillStart;
            _landHeight = landHeight;
            _hillWidth = hillWidth;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainHighland(generator, x, y, river, _hillStart, _hillWidth, _landHeight, BaseHeight - generator.Preset.SeaLevel);
        }
    }
}