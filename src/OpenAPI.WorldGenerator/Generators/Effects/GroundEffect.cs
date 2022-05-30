using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    /**
     * @author Zeno410
     */
    public class GroundEffect : HeightEffect
    {
        private float Amplitude { get; }

        public GroundEffect(float amplitude)
        {
            this.Amplitude = amplitude;
        }

        public override float Added(OverworldGeneratorV2 generator, float x, float y)
        {
            return TerrainBase.GetGroundNoise(generator, x, y, Amplitude);
        }
    }
}