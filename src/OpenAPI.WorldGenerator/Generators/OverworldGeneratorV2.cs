using System;
using System.ComponentModel;
using System.Linq;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Biomes.Vanilla.Ocean;
using OpenAPI.WorldGenerator.Generators.Decorators;
using OpenAPI.WorldGenerator.Generators.Terrain;
using OpenMiNET.Noise;
using OpenMiNET.Noise.Cellular;
using OpenMiNET.Noise.Modules;
using OpenMiNET.Noise.Primitives;

namespace OpenAPI.WorldGenerator.Generators
{
    [DisplayName("Overworld")]
    public class OverworldGeneratorV2 : IWorldGenerator
    {
        public static readonly float ActualRiverProportion = 150f / 1600f;//This value is also used in BiomeAnalyser#riverAdjusted
        public static readonly float RiverFlatteningAddend = ActualRiverProportion / (1f - ActualRiverProportion);
        public static readonly float RiverLargeBendSizeBase = 140f;
        public static readonly float RiverSmallBendSizeBase = 30f;
        public static readonly float RiverSeparationBase = 975f;
        public static readonly float RiverValleyLevelBase = 140f / 450f;
        public static readonly float LakeFrequencyBase = 649.0f;
        public static readonly float LakeShoreLevelBase = 0.035f;
        public static readonly float LakeDepressionLevelBase = 0.15f; // the lakeStrength below which land should start to be lowered
        public static readonly float LakeWaterLevelBase = 0.11f;
        public static readonly float LakeBendSizeLarge = 80;
        public static readonly float LakeBendSizeMedium = 30;
        public static readonly float LakeBendSizeSmall = 12;
        public static readonly int SimplexInstanceCount = 10;
        public static readonly int CellularInstanceCount = 5;

        public static float BlendRadius { get; set; } = 16f;


        public WorldGeneratorPreset Preset { get; }
        public BiomeRegistry BiomeRegistry { get; set; } 
        
        private readonly SimplexPerlin[] _simplexNoiseInstances = new SimplexPerlin[SimplexInstanceCount];
        private readonly SpacedCellularNoise[] _cellularNoiseInstances = new SpacedCellularNoise[CellularInstanceCount];

        public const     int       SampleSize      = 8;
        public const     int       SampleArraySize = SampleSize * 2 + 5;
        private readonly float[][] _weightings     = new float[SampleArraySize * SampleArraySize][];
        //private readonly int[] _biomeData = new int[SampleArraySize * SampleArraySize];
        
       // private BiomeBase[] BiomeList { get; set; }
        private bool[] BeachBiome { get; }
        private bool[] LandBiomes { get; }
        private bool[] WaterBiomes { get; }
        private readonly int _seed;
        public OverworldGeneratorV2()
        {
            Preset = new WorldGeneratorPreset();
            //Preset.BiomeSize = 1f;
           // Preset = JsonConvert.DeserializeObject<WorldGeneratorPreset>("{\"coordinateScale\":175.0,\"heightScale\":75.0,\"lowerLimitScale\":512.0,\"upperLimitScale\":512.0,\"depthNoiseScaleX\":200.0,\"depthNoiseScaleZ\":200.0,\"depthNoiseScaleExponent\":0.5,\"mainNoiseScaleX\":165.0,\"mainNoiseScaleY\":106.61267,\"mainNoiseScaleZ\":165.0,\"baseSize\":8.267606,\"stretchY\":13.387607,\"biomeDepthWeight\":1.2,\"biomeDepthOffset\":0.2,\"biomeScaleWeight\":3.4084506,\"biomeScaleOffset\":0.0,\"seaLevel\":63,\"useCaves\":true,\"useDungeons\":true,\"dungeonChance\":7,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useMonuments\":true,\"useRavines\":true,\"useWaterLakes\":true,\"waterLakeChance\":49,\"useLavaLakes\":true,\"lavaLakeChance\":80,\"useLavaOceans\":false,\"fixedBiome\":-1,\"biomeSize\":8,\"riverSize\":5,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":256,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":256,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisCenterHeight\":16,\"lapisSpread\":16}");
           // Preset = JsonConvert.DeserializeObject<WorldGeneratorPreset>(
            //          "{\"useCaves\":true,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useRavines\":true,\"useMonuments\":true,\"useMansions\":true,\"useLavaOceans\":false,\"useWaterLakes\":true,\"useLavaLakes\":true,\"useDungeons\":true,\"fixedBiome\":-3,\"biomeSize\":4,\"seaLevel\":63,\"riverSize\":4,\"waterLakeChance\":4,\"lavaLakeChance\":80,\"dungeonChance\":8,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":255,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":255,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisMinHeight\":0,\"lapisMaxHeight\":32,\"coordinateScale\":684,\"heightScale\":684,\"mainNoiseScaleX\":80,\"mainNoiseScaleY\":160,\"mainNoiseScaleZ\":80,\"depthNoiseScaleX\":200,\"depthNoiseScaleZ\":200,\"depthNoiseScaleExponent\":0.5,\"biomeDepthWeight\":1,\"biomeDepthOffset\":0,\"biomeScaleWeight\":1,\"biomeScaleOffset\":1,\"lowerLimitScale\":512,\"upperLimitScale\":512,\"baseSize\":8.5,\"stretchY\":12,\"lapisCenterHeight\":16,\"lapisSpread\":16}");;
                
            int seed = 356556635;
            _seed = seed;
            
            InitBiomeProviders(seed);

            for (int i = 0; i < SimplexInstanceCount; i++) {
                this._simplexNoiseInstances[i] = new SimplexPerlin(seed * i, NoiseQuality.Fast);
            }
            for (int i = 0; i < CellularInstanceCount; i++) {
                this._cellularNoiseInstances[i] = new SpacedCellularNoise(seed * i);
            }

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


            var totalBiomes = BiomeRegistry.Biomes.ToArray();
            
            BeachBiome = new bool[256];
            LandBiomes = new bool[256];
            WaterBiomes= new bool[256];

            foreach (var biome in totalBiomes)
            {
                BeachBiome[biome.Id] = ((biome.Type & BiomeType.Beach) != 0);
                LandBiomes[biome.Id] = (biome.Type & BiomeType.Land) != 0;
                WaterBiomes[biome.Id] = (biome.Type & BiomeType.Ocean) != 0 || (biome.Type & BiomeType.River) != 0;
            }
        }

        public float RiverSeperation => RiverSeparationBase / Preset.RiverFrequency;
        public float RiverSmallBendSize => RiverSmallBendSizeBase * Preset.RiverBendMult;
        public float RiverLargeBendSize => RiverLargeBendSizeBase * Preset.RiverBendMult;
        public float RiverValleyLevel => RiverValleyLevelBase * Preset.RiverSizeMult * Preset.RiverFrequency;

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
            var distortNoise = new MultiFractalNoiseModule(new SimplexPerlin(seed ^ 32, NoiseQuality.Fast));
            
            BiomeRegistry = new BiomeRegistry();
            var biomeScale = 16f * Preset.BiomeSize;

            INoiseModule temperatureNoise = new VoronoiNoseModule(new SimplexPerlin(seed ^ 3, NoiseQuality.Fast))
            {
                Distance = false, Frequency = 0.00325644f, Displacement = 2f
            };
            
       //     temperatureNoise = new TurbulenceNoiseModule(temperatureNoise, distortNoise, distortNoise, distortNoise, 1.25f);
            
            TemperatureNoise = new ScaledNoiseModule(temperatureNoise)
            {
                ScaleX = 1f / biomeScale, ScaleY = 1f / biomeScale, ScaleZ = 1f / biomeScale
            };

            RainfallNoise = new ScaledNoiseModule(new VoronoiNoseModule(new SimplexPerlin(seed * seed ^ 2, NoiseQuality.Fast))
            {
                Distance = false,
                Frequency = 0.0022776f,
                Displacement = 1f
            })
            {
                ScaleX = 1f / biomeScale, ScaleY = 1f / biomeScale, ScaleZ = 1f / biomeScale
            };

            INoiseModule selectorNoise = new SimplexPerlin(seed * 69, NoiseQuality.Fast);
           
            /*selectorNoise = new VoronoiNoseModule(selectorNoise)
            {
              Frequency = .845776f,
              Distance = false,
              Displacement = 1
              //OctaveCount = 2,
             // Offset = 1f,
             // Lacunarity = 1f,
             // Gain = 2f,
             // SpectralExponent = 0.9f
            };*/
           
           // selectorNoise = new TurbulenceNoiseModule(selectorNoise, distortNoise, distortNoise, distortNoise, 4.25f);
          // selectorNoise = new TurbulenceNoiseModule(distortNoise, distortNoise, distortNoise, distortNoise, 4.25f);
         // selectorNoise = new SimplexPerlin(seed * 69, NoiseQuality.Fast);
            SelectorNoise = new ScaledNoiseModule(new BillowNoiseModule(selectorNoise)
            {
                Scale = 1f / 6f
            })
            {
                ScaleX = 1f / 128f, ScaleY = 1f /128f, ScaleZ = 1f / 128f,
                OctaveCount = 4,
                 Offset = 1f,
                 Lacunarity = 1.3f,
                 Gain = 2f,
                 SpectralExponent = 0.9f,
                 Frequency = 0.01245776f
            };

            //SelectorNoise = selectorNoise;
            var d = new FoliageDecorator(Preset);
            d.SetSeed(seed);
            
            Decorators = new ChunkDecorator[] {d};
        }

        public  INoiseModule TemperatureNoise { get; set; }
        public  INoiseModule RainfallNoise    { get; set; }
        public INoiseModule SelectorNoise { get; set; }

        /// <inheritdoc />
        public void Initialize(IWorldProvider worldProvider)
        {
            
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            ChunkColumn column = new ChunkColumn() {X = chunkCoordinates.X, Z = chunkCoordinates.Z};

            ChunkLandscape landscape = new ChunkLandscape();
            CalculateBiomes(chunkCoordinates, landscape);

            GenerateTerrain(column, landscape.Noise);
            
            SetBiomeBlocks(column, landscape);

            return column;
        }

        #region Biome Selection

        private long                _totalLookups = 0;

        public float MaxTemp = float.MinValue;
        public float MinTemp = float.MaxValue;
        
        public float MaxRain = float.MinValue;
        public float MinRain = float.MaxValue;
        
        public float MaxSelector = float.MinValue;
        public float MinSelector = float.MaxValue;
        
        public float MaxHeight = float.MinValue;
        public float MinHeight = float.MaxValue;

        private void CalculateBiomes(ChunkCoordinates coordinates, ChunkLandscape landscape)
        {
            int worldX = coordinates.X << 4;
            int worldZ = coordinates.Z << 4;
          
            float[] weightedBiomes = new float[256];

            int[] biomes = new int[SampleArraySize * SampleArraySize];
            
            int GetIndex(int x, int z)
            {
                return  (x + SampleSize) * SampleArraySize + (z + SampleSize);
            }

            for (int x = -SampleSize; x < SampleSize + 5; x++)
            {
                for (int z = -SampleSize; z < SampleSize + 5; z++)
                {
                    var xx   = worldX + ((x * 8f));
                    var zz   = worldZ + ((z * 8f));

                    var temp = MathF.Abs(TemperatureNoise.GetValue(xx, zz));
                    MaxTemp = MathF.Max(temp, MaxTemp);
                    MinTemp = MathF.Min(temp, MinTemp);
                    
                    var rain = MathF.Abs(RainfallNoise.GetValue(xx, zz));
                    MaxRain = MathF.Max(rain, MaxRain);
                    MinRain = MathF.Min(rain, MinRain);
                    
                    _totalLookups++;

                    var selector =  MathF.Abs(SelectorNoise.GetValue(xx, zz));
                    MaxSelector = MathF.Max(selector, MaxSelector);
                    MinSelector = MathF.Min(selector, MinSelector);

                    var index = GetIndex(x, z);
                    
                     biomes[index] = (BiomeRegistry.GetBiome(
                          (float) temp, (float) rain,  selector));
                }
            }
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

                   float river = TerrainBase.GetRiverStrength(this, worldX + x, worldZ + z);
                   landscape.River[index] = -river;

                   float maxWeight = 0f;
                   for (int i = 0; i < weightedBiomes.Length; i++)
                   {
                       var value = weightedBiomes[i];

                       if (value > 0f)
                       {
                           var biome = BiomeRegistry.GetBiome(i);

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
                }
            }
        }
        
        public const int MAX_BIOMES = 256;

        private void SetBiomeBlocks(ChunkColumn chunk, ChunkLandscape landscape)
        {
            var jitteredBiomes = landscape.Biome;
            int worldX = chunk.X * 16;
            int worldZ = chunk.Z * 16;

            for (int x = 0; x < 16; x++)
            {
                var cx = worldX + x;
                for (int z = 0; z < 16; z++)
                {
                    var cz = worldZ + z;

                    var index = NoiseMap.GetIndex(x, z);
                    var localRiver = landscape.River[index];
                    int depth = -1;

                    jitteredBiomes[index].Replace(chunk, cx, cz, x, z, depth, this, landscape.Noise, localRiver, landscape.Biome);
                }
            }
            
            
            for (int x = 0; x < 16; x++)
            {
                var cx = worldX + x;
                for (int z = 0; z < 16; z++)
                {
                    var cz = worldZ + z;
                    var index = NoiseMap.GetIndex(x, z);

                    var biome = landscape.Biome[index];
                    var river = landscape.River[index];
                    
                    if (river > 0.9f && biome.Config.AllowRivers)
                    {
                        chunk.biomeId[index] = (byte) biome.GetRiverBiome().Id;
                        biome.GetRiverBiome().Decorate(chunk, landscape.Noise[index], river);
                    }
                    else
                    {
                        chunk.biomeId[index] = (byte) landscape.Biome[index].Id;
                        biome.Decorate(chunk, landscape.Noise[index], river);
                    }

                    foreach (var decorator in Decorators)
                    {
                        decorator.Decorate(
                            chunk, chunk.X, chunk.Z, biome, Array.Empty<float>(), x,
                            chunk.height[index], z, true, false);
                    }
                }
            }
        }

        #endregion
        
        private static int _stoneId = new Stone().GetRuntimeId();
        private static int _waterId = new Water().GetRuntimeId();
        private void GenerateTerrain(ChunkColumn column, float[] noiseMap)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var height = (int) noiseMap[NoiseMap.GetIndex(x, z)];

                    for (int y = 0; (y <= Math.Max(height, Preset.SeaLevel)) && y < 255; y++)
                    {
                        if (y <= height)
                        {
                            column.SetBlockByRuntimeId(x, y, z, _stoneId);
                        }
                        else if (y < Preset.SeaLevel)
                        {
                            column.SetBlockByRuntimeId(x, y, z, _waterId);
                        }
                    }


                    column.SetHeight(x, z, (short) height);
                }
            }
        }
    }
}