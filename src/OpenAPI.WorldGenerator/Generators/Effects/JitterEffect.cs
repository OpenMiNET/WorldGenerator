using System;
using OpenAPI.WorldGenerator.Utils.Noise;
using OpenAPI.WorldGenerator.Utils.Noise.Api;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    public class JitterEffect : HeightEffect
    {
        public float Amplitude  { get; set; } = int.MaxValue;
        public float Wavelength { get; set; } = 0;
        public HeightEffect Jittered { get; set; }

        public JitterEffect() {

        }

        public JitterEffect(float amplitude, float wavelength, HeightEffect toJitter)
        {
            this.Amplitude = amplitude;
            this.Wavelength = wavelength;
            this.Jittered = toJitter;
        }

        public override float Added(OverworldGeneratorV2 generator, float x, float y) {

            ISimplexData2D jitterData = new SimplexData2D();
            generator.SimplexInstance(1).GetValue(x / Wavelength, y / Wavelength, jitterData);
            int pX = (int) Math.Round(x + jitterData.GetDeltaX() * Amplitude);
            int pY = (int) Math.Round(y + jitterData.GetDeltaY() * Amplitude);
            return Jittered.Added(generator, pX, pY);
        }
    }
}