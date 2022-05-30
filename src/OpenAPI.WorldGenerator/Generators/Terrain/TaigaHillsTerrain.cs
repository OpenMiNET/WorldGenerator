namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class TaigaHillsTerrain : TerrainBase
    {
        private float HillStrength { get; set; } = 30f;
        public TaigaHillsTerrain() : this(72f, 30f)
        {
            
        }
        
        public TaigaHillsTerrain(float baseHeight, float hillStrength) : base(baseHeight)
        {
            HillStrength = hillStrength;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainHighland(generator, x, y, river, 10f, 68f, HillStrength, BaseHeight /*- generator.Preset.SeaLevel*/);
        }
    }
}