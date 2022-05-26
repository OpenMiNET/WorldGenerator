using OpenAPI.WorldGenerator.Generators.Biomes;

namespace OpenAPI.WorldGenerator.Generators
{
    public class ChunkLandscape
    {
        public readonly float[] Noise = new float[256];
        public readonly BiomeBase[] Biome = new BiomeBase[256];
        public readonly float[] River = new float[256];
    }
}