namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class SavannaPlateauTerrain : TerrainBase
    {
        private bool    _booRiver;
        private float[] _height;
        private int     _heightLength;
        private float   _strength;
        private float   _smooth;
        private float   _canyonWidth;
        private float   _canyonHeight;
        private float   _canyonStrength;
        private float   _baseVal;
        private bool    _isModified;
        
        public SavannaPlateauTerrain(bool riverGen, float heightStrength, float canyonWidth, float canyonHeight, float canyonStrength, float baseHeight, bool isModified) : base(baseHeight)
        {
            _booRiver = true;
            
            _height = new float[]{12.0f, 0.5f, 6f, 0.7f};
            _strength = heightStrength;
            _heightLength = _height.Length;

            _canyonWidth = canyonWidth;
            _canyonHeight = canyonHeight;
            _canyonStrength = canyonStrength;
            _isModified = isModified;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int x, int y, float border, float river)
        {
            return TerrainPlateau(generator, x, y, river, _height, border, _strength, _heightLength, 50f, _isModified);
        }
    }
}