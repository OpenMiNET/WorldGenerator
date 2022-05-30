using System;
using OpenAPI.WorldGenerator.Utils.Noise.Attributes;

namespace OpenAPI.WorldGenerator.Utils.Noise.Modules
{
  public class FilterNoise
  {
    public const float DEFAULT_FREQUENCY = 1f;
    public const float DEFAULT_LACUNARITY = 2f;
    public const float DEFAULT_OCTAVE_COUNT = 6f;
    public const int MAX_OCTAVE = 30;
    public const float DEFAULT_OFFSET = 1f;
    public const float DEFAULT_GAIN = 2f;
    public const float DEFAULT_SPECTRAL_EXPONENT = 0.9f;
    
    protected float _frequency = DEFAULT_FREQUENCY;
    protected float _gain = DEFAULT_GAIN;
    protected float _lacunarity = DEFAULT_LACUNARITY;
    protected float _octaveCount = DEFAULT_OCTAVE_COUNT;
    protected float _offset = DEFAULT_OFFSET;
    protected float _spectralExponent = DEFAULT_SPECTRAL_EXPONENT;
    protected float[] _spectralWeights = new float[30];
    

    protected INoiseModule _source;

    /// <summary>
    /// The number of cycles per unit length that a specific coherent-noise function outputs.
    /// </summary>
    [Modifier]
    public float Frequency
    {
      get { return this._frequency; }
      set { this._frequency = value; }
    }

    /// <summary>
    ///   A multiplier that determines how quickly the frequency increases for each successive octave in a Perlin-noise function.
    /// </summary>
    [Modifier]
    public float Lacunarity
    {
      get { return this._lacunarity; }
      set
      {
        this._lacunarity = value;
        this.ComputeSpectralWeights();
      }
    }

    [Modifier]
    public float OctaveCount
    {
      get { return this._octaveCount; }
      set { this._octaveCount = Math.Clamp(value, 1f, MAX_OCTAVE); }
    }

    [Modifier]
    public float Offset
    {
      get { return this._offset; }
      set { this._offset = value; }
    }

    [Modifier]
    public float Gain
    {
      get { return this._gain; }
      set { this._gain = value; }
    }

    [Modifier]
    public float SpectralExponent
    {
      get { return this._spectralExponent; }
      set
      {
        this._spectralExponent = value;
        this.ComputeSpectralWeights();
      }
    }

    public INoiseModule Primitive
    {
      get { return this._source; }
      set { this._source = value; }
    }

    protected FilterNoise()
      : this(1f, 2f, 0.9f, 6f)
    {
    }

    protected FilterNoise(float frequency, float lacunarity, float exponent, float octaveCount)
    {
      this._frequency = frequency;
      this._lacunarity = lacunarity;
      this._spectralExponent = exponent;
      this._octaveCount = octaveCount;
      this.ComputeSpectralWeights();
    }

    protected void ComputeSpectralWeights()
    {
      for (int index = 0; index < 30; ++index)
        this._spectralWeights[index] = MathF.Pow(this._lacunarity, -index * this._spectralExponent);
    }
  }
}