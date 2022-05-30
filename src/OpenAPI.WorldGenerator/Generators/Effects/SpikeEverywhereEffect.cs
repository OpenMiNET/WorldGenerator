using System;
using OpenAPI.WorldGenerator.Generators.Terrain;

namespace OpenAPI.WorldGenerator.Generators.Effects
{
    /// <summary>
    ///     Creates a spiky multiplier going from 0 to 1
    /// </summary>
    public class SpikeEverywhereEffect : HeightEffect
    {
        // not going to bother to set up a creator shell to make sure everything is set
        // set defaults to absurd values to crash if they're not set
        // a trio of parameters frequently used together

        public float Wavelength { get; set; } = 0;

        public float MinimumSimplex { get; set; } = int.MaxValue; // normal range is -1 to 1;

        //usually numbers above 0 are often preferred to avoid dead basins
        public int Octave { get; set; }
        public float Power { get; set; } = 1.6f; // usually a range of 1 to 2
        public HeightEffect Spiked { get; set; }

        public override float Added(OverworldGeneratorV2 generator, float x, float y)
        {
            float noise = generator.SimplexInstance(Octave).GetValue(x / Wavelength, y / Wavelength);
            noise = MathF.Abs(noise);
            noise = TerrainBase.BlendedHillHeight(noise, MinimumSimplex);
            noise = TerrainBase.UnsignedPower(noise, Power);
            return noise * Spiked.Added(generator, x, y);
        }
    }
}