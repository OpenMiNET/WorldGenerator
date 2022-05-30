namespace OpenAPI.WorldGenerator.Generators.Biomes.Config
{
    public class BiomeConfig
    {
        public bool SurfaceBlendIn { get; set; } = true;
        public bool SurfaceBlendOut { get; set; } = true;
        
        public bool AllowRivers { get; set; } = true;
        public bool AllowScenicLakes { get; set; } = true;

        public bool IsEdgeBiome { get; set; } = false;

        public int Weight { get; set; } = Weights.Common;
        
        public BiomeConfig()
        {
            
        }
    }

    public static class Weights
    {
        public const int Common = 10;
        public const int Uncommon = 8;
        public const int Rare = 5;
        public const int SuperRare = 3;
        public const int ExtremelyRare = 1;
    }
}