using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    /**
     * @author Zeno410
     */
    public class GroundEffect : HeightEffect
    {

        // the standard ground effect
        private float amplitude;

        public GroundEffect(float amplitude)
        {
            this.amplitude = amplitude;
        }

        public override float Added(OverworldGeneratorV2 generator, float x, float y)
        {
            return TerrainBase.GetGroundNoise(generator, x, y, amplitude);
        }

    }
}