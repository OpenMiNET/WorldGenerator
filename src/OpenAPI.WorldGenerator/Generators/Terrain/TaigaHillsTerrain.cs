namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class TaigaHillsTerrain : TerrainBase
    {
        private float HillStrength { get; set; }
        public TaigaHillsTerrain(float baseHeight = 72f, float hillStrength = 30f) : base(baseHeight)
        {
            HillStrength = hillStrength;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainHighland(generator, x, y, river, 10f, 68f, HillStrength, BaseHeight - generator.Preset.SeaLevel);
        }
    }
}