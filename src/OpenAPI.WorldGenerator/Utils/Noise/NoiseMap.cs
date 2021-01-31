namespace OpenAPI.WorldGenerator.Utils.Noise
{
    public class NoiseMap
    {
        private float[] HeightMap { get; }
        
        public NoiseMap(float[] noiseMap)
        {
            HeightMap = noiseMap;
        }

        public float[] Data
        {
            get { return HeightMap; }
        }

        public float this[int index]
        {
            get { return HeightMap[index]; }
            set { HeightMap[index] = value; }
        }

        public float Get(int x, int z)
        {
            return HeightMap[GetIndex(x, z)];
        }

        public void Set(int x, int z, float value)
        {
            HeightMap[GetIndex(x, z)] = value;
        }
        
        public static int GetIndex(int x, int z)
        {
            return (z << 4) + x;
        }
    }
}