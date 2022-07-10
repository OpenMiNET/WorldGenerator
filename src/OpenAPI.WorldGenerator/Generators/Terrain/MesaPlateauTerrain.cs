using OpenAPI.WorldGenerator.Generators.Effects;
using OpenAPI.WorldGenerator.Generators.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;

namespace OpenAPI.WorldGenerator.Generators.Terrain
{
    public class MesaPlateauTerrain : TerrainBase
    {
        private static float _stepStart = 0.25f;
        private static float _stepFinish = 0.4f;
        private static float _stepHeight = 32;
        private VoronoiPlateauEffect _plateau;
        private int _groundNoise;
        private float _jitterWavelength = 30;
        private float _jitterAmplitude = 10;
        private float _bumpinessMultiplier = 0.05f;
        private float _bumpinessWavelength = 10f;

        
        public MesaPlateauTerrain(float baseHeight) : base(baseHeight)
        {
            _plateau = new VoronoiPlateauEffect();
            _plateau.PointWavelength = 200;
            _groundNoise = 4;
        }
        
        public override float GenerateNoise(OverworldGeneratorV2 generator, int passedX, int passedY, float border, float river)
        {
            var jitterData = new SimplexData2D();
            generator.SimplexInstance(1).GetValue(passedX / _jitterWavelength, passedY / _jitterWavelength, jitterData);
            float x = (float) (passedX + jitterData.GetDeltaX() * _jitterAmplitude);
            float y = (float) (passedY + jitterData.GetDeltaY() * _jitterAmplitude);
            float bordercap = (bordercap = border * 3.5f - 2.5f) > 1 ? 1.0f : bordercap;
            float rivercap = (rivercap = 3f * river) > 1 ? 1.0f : rivercap;
            float bumpiness = generator.SimplexInstance(2).GetValue(x / _bumpinessWavelength, y / _bumpinessWavelength) * _bumpinessMultiplier;
            float simplex = _plateau.Added(generator, x, y) * bordercap * rivercap + bumpiness;
            float added = PlateauUtil.StepIncrease(simplex, _stepStart, _stepFinish, _stepHeight) / border;

            /*return TerrainPlateau(
                generator, x, y, river, new float[] {12.0f, 0.5f, 6f, 0.7f}, border, 35f, 4, 50f, false);*/
            return Riverized(generator, BaseHeight + GetGroundNoise(generator, x, y, _groundNoise), river) + added;
        }
    }
}