using System;
using System.ComponentModel;
using System.Linq;
using LibNoise;
using LibNoise.Filter;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean;
using OpenAPI.WorldGenerator.Generators.Decorators;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenAPI.WorldGenerator.Utils.Noise;
using OpenAPI.WorldGenerator.Utils.Noise.Api;
using OpenAPI.WorldGenerator.Utils.Noise.Cellular;
using OpenAPI.WorldGenerator.Utils.Noise.Modules;
using OpenAPI.WorldGenerator.Utils.Noise.Primitives;

namespace OpenAPI.WorldGenerator.Generators
{
    [DisplayName("Overworld")]
    public class OverworldGeneratorV2 : IWorldGenerator
    {
        public static readonly float ActualRiverProportion = 150f / 1600f;//This value is also used in BiomeAnalyser#riverAdjusted
        public static readonly float RiverFlatteningAddend = ActualRiverProportion / (1f - ActualRiverProportion);
        public static readonly double RiverLargeBendSizeBase = 140D;
        public static readonly double RiverSmallBendSizeBase = 30D;
        public static readonly double RiverSeparationBase = 975D;
        public static readonly double RiverValleyLevelBase = 140D / 450D;
        public static readonly float LakeFrequencyBase = 649.0f;
        public static readonly float LakeShoreLevelBase = 0.035f;
        public static readonly float LakeDepressionLevelBase = 0.15f; // the lakeStrength below which land should start to be lowered
        public static readonly float LakeWaterLevelBase = 0.11f;
        public static readonly float LakeBendSizeLarge = 80;
        public static readonly float LakeBendSizeMedium = 30;
        public static readonly float LakeBendSizeSmall = 12;
        public static readonly int SimplexInstanceCount = 10;
        public static readonly int CellularInstanceCount = 5;

        public static float BlendRadius { get; set; } = 8f;


        public WorldGeneratorPreset Preset { get; }
        public BiomeProvider BiomeProvider { get; set; } 
        
        private readonly SimplexPerlin[] _simplexNoiseInstances = new SimplexPerlin[SimplexInstanceCount];
        private readonly SpacedCellularNoise[] _cellularNoiseInstances = new SpacedCellularNoise[CellularInstanceCount];

        public const     int       SampleSize      = 8;
        public const     int       SampleArraySize = SampleSize * 2 + 5;
        private readonly float[][] _weightings     = new float[SampleArraySize * SampleArraySize][];
        //private readonly int[] _biomeData = new int[SampleArraySize * SampleArraySize];
        
       // private BiomeBase[] BiomeList { get; set; }
        private bool[] BeachBiome { get; }
        private bool[] LandBiome { get; }
        private bool[] OceanBiome { get; }
        private readonly int _seed;
        public OverworldGeneratorV2()
        {
            Preset = new WorldGeneratorPreset();
            //Preset.BiomeSize = 1f;
           // Preset = JsonConvert.DeserializeObject<WorldGeneratorPreset>("{\"coordinateScale\":175.0,\"heightScale\":75.0,\"lowerLimitScale\":512.0,\"upperLimitScale\":512.0,\"depthNoiseScaleX\":200.0,\"depthNoiseScaleZ\":200.0,\"depthNoiseScaleExponent\":0.5,\"mainNoiseScaleX\":165.0,\"mainNoiseScaleY\":106.61267,\"mainNoiseScaleZ\":165.0,\"baseSize\":8.267606,\"stretchY\":13.387607,\"biomeDepthWeight\":1.2,\"biomeDepthOffset\":0.2,\"biomeScaleWeight\":3.4084506,\"biomeScaleOffset\":0.0,\"seaLevel\":63,\"useCaves\":true,\"useDungeons\":true,\"dungeonChance\":7,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useMonuments\":true,\"useRavines\":true,\"useWaterLakes\":true,\"waterLakeChance\":49,\"useLavaLakes\":true,\"lavaLakeChance\":80,\"useLavaOceans\":false,\"fixedBiome\":-1,\"biomeSize\":8,\"riverSize\":5,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":256,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":256,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisCenterHeight\":16,\"lapisSpread\":16}");
           // Preset = JsonConvert.DeserializeObject<WorldGeneratorPreset>(
            //          "{\"useCaves\":true,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useRavines\":true,\"useMonuments\":true,\"useMansions\":true,\"useLavaOceans\":false,\"useWaterLakes\":true,\"useLavaLakes\":true,\"useDungeons\":true,\"fixedBiome\":-3,\"biomeSize\":4,\"seaLevel\":63,\"riverSize\":4,\"waterLakeChance\":4,\"lavaLakeChance\":80,\"dungeonChance\":8,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":255,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":255,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisMinHeight\":0,\"lapisMaxHeight\":32,\"coordinateScale\":684,\"heightScale\":684,\"mainNoiseScaleX\":80,\"mainNoiseScaleY\":160,\"mainNoiseScaleZ\":80,\"depthNoiseScaleX\":200,\"depthNoiseScaleZ\":200,\"depthNoiseScaleExponent\":0.5,\"biomeDepthWeight\":1,\"biomeDepthOffset\":0,\"biomeScaleWeight\":1,\"biomeScaleOffset\":1,\"lowerLimitScale\":512,\"upperLimitScale\":512,\"baseSize\":8.5,\"stretchY\":12,\"lapisCenterHeight\":16,\"lapisSpread\":16}");;
                
            int seed = 1434234;
            _seed = seed;
            
            InitBiomeProviders(seed);

            for (int i = 0; i < SimplexInstanceCount; i++) {
                this._simplexNoiseInstances[i] = new SimplexPerlin(seed + i, NoiseQuality.Fast);
            }
            for (int i = 0; i < CellularInstanceCount; i++) {
                this._cellularNoiseInstances[i] = new SpacedCellularNoise(seed + i);
            }

   //         BiomeList = new BiomeBase[256];
            
            for (int i = 0; i < _weightings.Length; i++)
            {
                _weightings[i] = new float[256];
            }
            
            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++) {
                    float limit = MathF.Pow((56f * 56f), 0.7F);
                    for (int mapX = 0; mapX < SampleArraySize; mapX++) {
                        for (int mapZ = 0; mapZ < SampleArraySize; mapZ++) {
                            float xDist = (x - (mapX - SampleSize) * 8);
                            float zDist = (z - (mapZ - SampleSize) * 8);
                            float distanceSquared = xDist * xDist + zDist * zDist;
                            float distance = MathF.Pow(distanceSquared, 0.7F);
                            float weight = 1f - distance / limit;
                            if (weight < 0) {
                                weight = 0;
                            }
                            
                            _weightings[mapX * SampleArraySize + mapZ][x * 16 + z] = weight;
                        }
                    }
                }
            }


            var totalBiomes = BiomeProvider.Biomes.ToArray();
            
            BeachBiome = new bool[256];
            LandBiome = new bool[256];
            OceanBiome= new bool[256];
            foreach (var biome in totalBiomes)
            {
                BeachBiome[biome.Id] = ((biome.Type & BiomeType.Beach) != 0);
                LandBiome[biome.Id] = (biome.Type & BiomeType.Land) != 0;
                OceanBiome[biome.Id] = (biome.Type & BiomeType.Ocean) != 0;
            }
           // BeachBiome = totalBiomes.Select(x => (x.Type.HasFlag(BiomeType.Beach))).ToArray();
           // LandBiome = totalBiomes.Select(x => (x.Type.HasFlag(BiomeType.Land))).ToArray();
          //  OceanBiome = totalBiomes.Select(x => (x.Type.HasFlag(BiomeType.Ocean))).ToArray();
        }

        public double RiverSeperation => RiverSeparationBase / Preset.RiverFrequency;
        public double RiverSmallBendSize => RiverSmallBendSizeBase * Preset.RiverBendMult;
        public double RiverLargeBendSize => RiverLargeBendSizeBase * Preset.RiverBendMult;
        public double RiverValleyLevel => RiverValleyLevelBase * Preset.RiverSizeMult * Preset.RiverFrequency;

        public float LakeFrequency => LakeFrequencyBase * Preset.RTGlakeFreqMult;
        public float LakeShoreLevel => LakeShoreLevelBase * Preset.RTGlakeFreqMult * Preset.RTGlakeSizeMult;
        
        public float LakeWaterLevel => LakeWaterLevelBase * Preset.RTGlakeFreqMult *
                                       Preset.RTGlakeSizeMult;  
        public float LakeDepressionLevel => LakeDepressionLevelBase * Preset.RTGlakeFreqMult *
                                            Preset.RTGlakeSizeMult;
        
        public SimplexPerlin SimplexInstance(int index) {
            if (index >= this._simplexNoiseInstances.Length) {
                index = 0;
            }
            return this._simplexNoiseInstances[index];
        }
        
        public SpacedCellularNoise CellularInstance(int index) {
            if (index >= this._cellularNoiseInstances.Length) {
                index = 0;
            }
            return this._cellularNoiseInstances[index];
        }

        private ChunkDecorator[] Decorators { get; set; }
        private void InitBiomeProviders(int seed)
        {
            var biomeScale = 128f;

            INoiseModule temperaturePrimitive = new SimplexPerlin(seed ^ 3, NoiseQuality.Fast);
            INoiseModule temperatureNoise = temperaturePrimitive;

            temperatureNoise = new ScaledNoiseModule(temperatureNoise)
            {
                ScaleX = 1f / biomeScale, ScaleY = 1f / biomeScale, ScaleZ = 1f / biomeScale
            };
            /*temperatureNoise = new BillowNoiseModule()
            {
                Primitive = temperatureNoise,
                Lacunarity = 2f,
                Frequency = 0.25f,
                OctaveCount = 1
            };*/
            temperatureNoise = new VoronoiNoseModule()
            {
                Primitive = temperatureNoise,
                Distance = true,
                Frequency = 0.0425644f,
                OctaveCount = 6,
               // Size = 8,
               
                Displacement = 2f,
                Gain = 1.3f,
                Lacunarity = 1.1f,
                SpectralExponent = 0.6f
            };

            INoiseModule rainNoise = new SimplexPerlin(seed * seed ^ 2, NoiseQuality.Fast);
            rainNoise = new ScaledNoiseModule(rainNoise)
            {
                ScaleX = 1f / biomeScale, ScaleY = 1f / biomeScale, ScaleZ = 1f / biomeScale
            };
            rainNoise = new VoronoiNoseModule()
            {
                Primitive = rainNoise,
                Distance = true,
                Frequency = 0.022776f,
                OctaveCount = 6,
                
                Displacement = 1f,
                Gain = 2.3f,
                Lacunarity = 1.3f,
                SpectralExponent = 0.8f
            };




            //   rainNoise = new TurbulenceNoiseModule(rainNoise, distortionY, distortionX, distortionX, 16f);

            BiomeProvider = new BiomeProvider();
            

            INoiseModule selectorNoise = new SimplexPerlin(seed, NoiseQuality.Fast);
            
            selectorNoise = new BillowNoiseModule()
            {
                Primitive = selectorNoise,
                Frequency = 0.0125644f,
                OctaveCount = 4,
                // Size = 8,
              //  Distance = false,
               // Displacement = 2f,
                Gain = 1.36f,
                Lacunarity = 1.1f,
                SpectralExponent = Preset.DepthNoiseScaleExponent
            };// new SimplexPerlin(seed * seed ^ 2, NoiseQuality.Fast);}

            selectorNoise = new ScaledNoiseModule(selectorNoise)
            {
                ScaleX = 1f / Preset.DepthNoiseScaleX,
                ScaleY = 1f / Preset.DepthNoiseScaleZ,
                ScaleZ = 1f / Preset.DepthNoiseScaleZ
            };
            
            var d = new FoliageDecorator(Preset);
            d.SetSeed(seed);

            RainfallNoise = rainNoise;
            TemperatureNoise = temperatureNoise;
            SelectorNoise = selectorNoise;
            Decorators = new ChunkDecorator[] {d};
        }

        public  INoiseModule TemperatureNoise { get; set; }
        public  INoiseModule RainfallNoise    { get; set; }
        public INoiseModule SelectorNoise { get; set; }

        public  bool         ApplyBlocks { get; set; } = true;

        /// <inheritdoc />
        public void Initialize(IWorldProvider worldProvider)
        {
            
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            ChunkColumn column = new ChunkColumn() {X = chunkCoordinates.X, Z = chunkCoordinates.Z};

            ChunkLandscape landscape = new ChunkLandscape();
            var biomes = CalculateBiomes(chunkCoordinates, landscape);
            
            GenerateTerrain(column, landscape.Noise);
            
            FixBiomes(column, landscape, biomes);

            SetBiomeBlocks(column, landscape);

            return column;
        }

        private float RiverAdjusted(float top, float river) {
            if (river >= 1) {
                return top;
            }
            float erodedRiver = river / ActualRiverProportion;
            if (erodedRiver <= 1f) {
                top = top * (1 - erodedRiver) + (Preset.SeaLevel - 1f) * erodedRiver;
            }
            top = top * (1 - river) + (Preset.SeaLevel - 1f) * river;
            return top;
        }

        private void SetBiomeBlocks(ChunkColumn chunk, ChunkLandscape landscape)
        {
            int worldX = chunk.X * 16;
            int worldZ = chunk.Z * 16;

            var coords = new BlockCoordinates(worldX, 0, worldZ);
            
            for (int x = 0; x < 16; x++)
            {
                coords.X = worldX + x;
                for (int z = 0; z < 16; z++)
                {
                    coords.Z = worldZ + z;

                    var index = NoiseMap.GetIndex(x, z);
                    var biome = landscape.Biome[index];
                    var river = landscape.River[index];
                    int depth = -1;

                    biome.Replace(chunk, coords.X, coords.Z, x, z, depth, this, landscape.Noise, river, landscape.Biome);
                    chunk.biomeId[index] = (byte) biome.Id;
                }
            }

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var index = NoiseMap.GetIndex(x, z);

                    var biome = landscape.Biome[index];
                    var river = landscape.River[index];
                    biome.Decorate(chunk, landscape.Noise[index], river);
                    
                    foreach (var decorator in Decorators)
                    {
                        decorator.Decorate(
                            chunk, chunk.X, chunk.Z, BiomeProvider.GetBiome(chunk.biomeId[index]), new float[0], x,
                            chunk.height[index], z, true, false);
                    }
                }
            }
        }
        
        #region Biome Selection

        private long                _totalLookups = 0;

        public float MaxTemp = float.MinValue;
        public float MinTemp = float.MaxValue;
        
        public float MaxRain = float.MinValue;
        public float MinRain = float.MaxValue;
        
      //  private ThreadSafeList<int> _uniqueBiomes  = new ThreadSafeList<int>();
        private int[] CalculateBiomes(ChunkCoordinates coordinates, ChunkLandscape landscape)
        {
           // var biomes = BiomeProvider.GetBiomes().ToArray();
            
            int worldX = coordinates.X << 4;
            int worldZ = coordinates.Z << 4;
          
            float[] weightedBiomes = new float[256];

            int[] biomes = new int[SampleArraySize * SampleArraySize];

            for (int x = -SampleSize; x < SampleSize + 5; x++)
            {
                for (int z = -SampleSize; z < SampleSize + 5; z++)
                {
                    var xx   = worldX + ((x * 8f));
                    var zz   = worldZ + ((z * 8f));
                    
                   var temp = MathF.Abs(TemperatureNoise.GetValue(xx, zz));
                   var rain = MathF.Abs(RainfallNoise.GetValue(xx, zz));
                  
                   //Note for self: The temperature noise returns a value between -1 & 1 however we need the value to be between 0 & 2.
                    //We do this by getting the absolute value (0 to 1) and multiplying it by 2.
                  //  temp = MathF.Abs(temp) * 2f;

                    if (temp > 2f)
                    {
                        var remainder = temp - 2f;
                        temp = 2f - remainder;
                    }
                    MaxTemp = MathF.Max(temp, MaxTemp);
                    MinTemp = MathF.Min(temp, MinTemp);
                    
                    if (rain > 1f)
                    {
                        var remainder = rain - 1f;
                        rain = 1f - remainder;
                    }
                    
                    MaxRain = MathF.Max(rain, MaxRain);
                    MinRain = MathF.Min(rain, MinRain);
                    
                    _totalLookups++;

                    biomes[(x + SampleSize) * SampleArraySize + (z + SampleSize)] = (BiomeProvider.GetBiome(
                        (float) temp, (float) rain,  SelectorNoise.GetValue(xx, zz))).Id;
                }
            }
            
       //    var availableBiomes = BiomeProvider.GetBiomes();
            
            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++)
                {
                    int index     = NoiseMap.GetIndex(x, z);
                    
                    float totalWeight = 0;
                    for (int mapX = 0; mapX < SampleArraySize; mapX++) {
                        for (int mapZ = 0; mapZ < SampleArraySize; mapZ++) {
                            float weight = _weightings[mapX * SampleArraySize + mapZ][(x << 4) + z];
                            if (weight > 0) {
                                totalWeight += weight;
                                weightedBiomes[biomes[mapX * SampleArraySize + mapZ]] += weight;
                            }
                        }
                    }

                    // normalize biome weights
                    for (int biomeIndex = 0; biomeIndex < weightedBiomes.Length; biomeIndex++) {
                        weightedBiomes[biomeIndex] /= totalWeight;
                    }

                    // combine mesa biomes
                   // mesaCombiner.adjust(weightedBiomes);
                   landscape.Noise[index] = 0f;

                   float river = TerrainBase.GetRiverStrength(worldX + x, worldZ + z, this);
                   landscape.River[index] = -river;

                   float maxWeight = 0f;
                   for (int i = 0; i < weightedBiomes.Length; i++)
                   {
                       var value = weightedBiomes[i];

                       if (value > 0f)
                       {
                           var biome = BiomeProvider.GetBiome(i);

                           landscape.Noise[index] += biome.TerrainNoise(
                               this, worldX + x, worldZ + z, value, river + 1f) * value;

                           weightedBiomes[i] = 0f;

                           if (value > maxWeight)
                           {
                               maxWeight = value;
                               landscape.Biome[index] = biome;
                           }
                       }
                   }
                   
                   //landscape.Biome[index] = weightedBiomes.;

                   // landscape.Biome[index] = BiomeProvider.GetBiome(i);
                   //landscape.Biome[index] = BiomeProvider.GetBiome();
                }
            }

            return biomes;
        }
        
        public const int MAX_BIOMES = 256;
        private void FixBiomes(ChunkColumn column,
            ChunkLandscape landscape, int[] neighboring)
        {
         //   return;
          //  landscape.Biome
           
            ISimplexData2D jitterData = SimplexData2D.NewDisk();
             BiomeBase[] jitteredBiomes = new BiomeBase[256];
             var riverStrength = landscape.River;
             var noise = landscape.Noise;
            BiomeBase jitterbiome, actualbiome;
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
                    int x = (column.X * 16) + i;
                    int z = (column.Z * 16) + j;
                    this.SimplexInstance(0).GetValue(x, z, jitterData);
                    int pX = (int) Math.Round(x + jitterData.GetDeltaX() * BlendRadius);
                    int pZ = (int) Math.Round(z + jitterData.GetDeltaY() * BlendRadius);
                    actualbiome = landscape.Biome[(x & 15) * 16 + (z & 15)];
                    jitterbiome = landscape.Biome[(pX & 15) * 16 + (pZ & 15)];
                    jitteredBiomes[i * 16 + j] =  (actualbiome.Config.SurfaceBlendIn && jitterbiome.Config.SurfaceBlendOut) ? jitterbiome : actualbiome;
                    riverStrength[i * 16 + j] = RiverAdjusted(Preset.SeaLevel + 2.5f, riverStrength[i * 16 + j]);
                }
            }


            BiomeBase realisticBiome;

            // currently just stuffs the genLayer into the jitter;
            for (int i = 0; i < MAX_BIOMES; i++) 
            {
                realisticBiome = landscape.Biome[i];
                
                var targetHeight = noise[i];
                
                bool canBeRiver = riverStrength[i] > 0.7 && targetHeight >= (Preset.SeaLevel * 0.5f) && targetHeight <= Preset.SeaLevel;
                if (targetHeight > Preset.SeaLevel) 
                {
                    // replace
                    jitteredBiomes[i] = realisticBiome;
                }
                else 
                {
                    // check for river
                    if (canBeRiver) {
                        // make river
                        jitteredBiomes[i] = BiomeProvider.GetBiome(realisticBiome.GetRiverBiome());
                    }
                    else if (targetHeight < Preset.SeaLevel)
                    {
                        jitteredBiomes[i] = realisticBiome;
                    }
                }
            }
            
            SmoothingSearchStatus beachSearch = new SmoothingSearchStatus(BeachBiome);
            SmoothingSearchStatus landSearch = new SmoothingSearchStatus(LandBiome);
            
            beachSearch.SetNotHunted();
            beachSearch.SetAbsent();
            
            landSearch.SetNotHunted();
            landSearch.SetAbsent();
        //  beachSearch.Hunt(neighboring);
        //    landSearch.Hunt(neighboring);

        float beachTop = Preset.SeaLevel + 2.5f;// 64.5f;
            for (int i = 0; i < MAX_BIOMES; i++)
            {
                float beachBottom = Preset.SeaLevel - 1.5f;
                
                if (noise[i] < beachBottom || noise[i] > RiverAdjusted(beachTop, riverStrength[i])) {
                    continue;// this block isn't beach level
                }

                if ((jitteredBiomes[i].Type & BiomeType.Swamp) != 0)
                {
                    continue;
                }

                if (landSearch.IsNotHunted())
                {
                //   beachSearch.Hunt(neighboring);
                    landSearch.Hunt(neighboring);
                    //landSearch.SetNotHunted();
                }
                
            //    int foundBeachBiome = beachSearch.Biomes[i];

             //   if (foundBeachBiome != -1)
                {
                 //   var nearestBeachBiome = BiomeProvider.GetBiome(foundBeachBiome);
                    int nearestLandBiome = landSearch.Biomes[i];

                    if (nearestLandBiome > -1)
                    {
                        var foundBiome = BiomeProvider.GetBiome(nearestLandBiome).GetBeachBiome();
                        var biome = BiomeProvider.GetBiome(foundBiome);

                        if (biome != null)
                        {
                            jitteredBiomes[i] = biome;
                        }
                    }
                }
            }

            for (int i = 0; i < jitteredBiomes.Length; i++)
            {
                landscape.Biome[i] = jitteredBiomes[i];
            }
        }
        
        #endregion
        
        private void GenerateTerrain(ChunkColumn column, float[] noiseMap)
        {
            int stoneId = new Stone().GetRuntimeId();
            var waterId = new Water().GetRuntimeId();
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var height = (int) noiseMap[NoiseMap.GetIndex(x, z)];

                    for (int y = 0; (y <= height || y <= Preset.SeaLevel) && y < 255; y++)
                    {
                        if (y > height)
                        {
                            if (y <= Preset.SeaLevel)
                            {
                                column.SetBlockByRuntimeId(x, y, z, waterId);
                            }
                        }
                        else if (y == height)
                        {
                            column.SetBlockByRuntimeId(x, y, z, stoneId);
                        }
                        else if (y < height)
                        {
                            column.SetBlockByRuntimeId(x, y, z, stoneId);
                        }
                    }


                    column.SetHeight(x, z, (short) height);
                }
            }
        }
    }
}