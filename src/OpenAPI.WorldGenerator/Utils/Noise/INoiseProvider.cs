using MiNET.Utils;
using MiNET.Utils.Vectors;

namespace OpenAPI.WorldGenerator.Utils.Noise
{
    public interface INoiseProvider
    {
        NoiseMap Get(ChunkCoordinates coordinates);
    }
}