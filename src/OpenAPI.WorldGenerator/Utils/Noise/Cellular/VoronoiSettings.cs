namespace OpenAPI.WorldGenerator.Utils.Noise.Cellular
{
    public class VoronoiSettings
    {
        public float ShortestDistance { get; set; } = 32000000.0f;
        public float NextDistance { get; set; } = 32000000.0f;
        public float ClosestX { get; set; } = 32000000.0f;
        public float ClosestZ { get; set; } = 32000000.0f;

        public VoronoiSettings() : this(32000000.0f)
        {
            
        }

        public VoronoiSettings(float defaultValues)
        {
            ShortestDistance = defaultValues;
            NextDistance = defaultValues;
            ClosestX = defaultValues;
            ClosestZ = defaultValues;
        }
    }
}