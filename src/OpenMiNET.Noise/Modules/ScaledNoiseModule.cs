using OpenMiNET.Noise.Attributes;

namespace OpenMiNET.Noise.Modules
{
    public class ScaledNoiseModule : FilterNoise, INoiseModule
    {
        public ScaledNoiseModule(INoiseModule noiseModule) : base(DEFAULT_FREQUENCY, 2f, 0.9f, 1f)
        {
            base.Primitive = noiseModule;
        }

        [Modifier]
        public float ScaleX { get; set; }
        
        [Modifier]
        public float ScaleY { get; set; }
        
        [Modifier]
        public float ScaleZ { get; set; }
        
        
        public float GetValue(float x, float y)
        {
            float freq = Frequency;

            float result = 0f;
            float max = 0f;
            x *= ScaleX;
            y *= ScaleZ;
            
            int curOctave;
            
            for (curOctave = 0; curOctave < _octaveCount; curOctave++)
            {
                var signal = _source.GetValue(x * freq, y * freq) * _spectralWeights[curOctave];
                result += signal;

                x *= _lacunarity;
                y *= _lacunarity;

                if (signal > max)
                    max = signal;
            }

            float remainder = _octaveCount - (int) _octaveCount;

            if (remainder > 0.0f)
                result += remainder*_source.GetValue(x, y)*_spectralWeights[curOctave];
            
            return result;
        }

        public float GetValue(float x, float y, float z)
        {
            float freq = Frequency;

            float result = 0f;
            x *= ScaleX;
            y *= ScaleY;
            y *= ScaleZ;
            
            int curOctave;

            for (curOctave = 0; curOctave < _octaveCount; curOctave++)
            {
                var signal = _source.GetValue(x * freq, y * freq, z * freq) * _spectralWeights[curOctave];
                result += signal;

                x *= _lacunarity;
                y *= _lacunarity;
                z *= _lacunarity;
            }

            float remainder = _octaveCount - (int) _octaveCount;

            if (remainder > 0.0f)
                result += remainder*_source.GetValue(x, y, z)*_spectralWeights[curOctave];
            
            return result / _octaveCount;
        }
    }
}