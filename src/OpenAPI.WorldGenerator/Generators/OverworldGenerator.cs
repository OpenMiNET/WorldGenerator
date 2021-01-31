using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using LibNoise;
using LibNoise.Filter;
using LibNoise.Modifier;
using LibNoise.Primitive;
using LibNoise.Transformer;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators.Biomes;
using OpenAPI.WorldGenerator.Generators.Decorators;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;
using Biome = OpenAPI.WorldGenerator.Utils.Biome;
using BiomeUtils = OpenAPI.WorldGenerator.Utils.BiomeUtils;

namespace OpenAPI.WorldGenerator.Generators
{
    [DisplayName("FastGen")]
    public class OverworldGenerator : IWorldGenerator
    {
        /*private readonly IModule3D _depthNoise;
        private readonly IModule2D _terrainNoise;
        private readonly IModule2D _rainNoise;
        private readonly IModule2D _tempNoise;

        private readonly IModule2D _baseHeightNoise;
        
        private float MainNoiseFrequency = 0.295f;
        private float MainNoiseLacunarity = 2.127f;
        private float MainNoiseGain = 2f;//0.256f;
        private float MainNoiseSpectralExponent = 0.5f;//1f;//0.52f;//0.9f;//1.4f;
        private float MainNoiseOffset = 0f;// 0.312f;

        private float TemperatureFrequency = 0.83f;
        private float RainFallFrequency = 1.03f;
        
        private float DepthFrequency = 0.662f;
        private float DepthLacunarity = 2.375f; //6f;
        private float DepthNoiseGain = 2f;//0.256f;*/
        
        private float MaxHeight = 256;
        public static float WaterLevel = 64;
        
        private NoiseProvider NoiseProvider { get; }
        private int Seed { get; }
        public OverworldGenerator()
        {
            BiomeUtils.FixMinMaxHeight();

            //GeneratorPreset = JsonConvert.DeserializeObject<WorldGeneratorPreset>("{\"coordinateScale\":450.0,\"heightScale\":1.0,\"lowerLimitScale\":1450.0,\"upperLimitScale\":1450.0,\"depthNoiseScaleX\":200.0,\"depthNoiseScaleZ\":200.0,\"depthNoiseScaleExponent\":0.5,\"mainNoiseScaleX\":1800.0,\"mainNoiseScaleY\":5000.0,\"mainNoiseScaleZ\":1800.0,\"baseSize\":9.275,\"stretchY\":4.005,\"biomeDepthWeight\":1.105,\"biomeDepthOffset\":0.01,\"biomeScaleWeight\":4.0,\"biomeScaleOffset\":0.01,\"seaLevel\":63,\"useCaves\":true,\"useDungeons\":true,\"dungeonChance\":10,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useMonuments\":true,\"useRavines\":true,\"useWaterLakes\":true,\"waterLakeChance\":100,\"useLavaLakes\":true,\"lavaLakeChance\":100,\"useLavaOceans\":false,\"fixedBiome\":-1,\"biomeSize\":5,\"riverSize\":5,\"dirtSize\":33,\"dirtCount\":12,\"dirtMinHeight\":0,\"dirtMaxHeight\":256,\"gravelSize\":33,\"gravelCount\":12,\"gravelMinHeight\":0,\"gravelMaxHeight\":256,\"graniteSize\":33,\"graniteCount\":12,\"graniteMinHeight\":0,\"graniteMaxHeight\":255,\"dioriteSize\":33,\"dioriteCount\":12,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":255,\"andesiteSize\":33,\"andesiteCount\":12,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":255,\"coalSize\":17,\"coalCount\":25,\"coalMinHeight\":0,\"coalMaxHeight\":255,\"ironSize\":9,\"ironCount\":25,\"ironMinHeight\":0,\"ironMaxHeight\":255,\"goldSize\":9,\"goldCount\":8,\"goldMinHeight\":0,\"goldMaxHeight\":255,\"redstoneSize\":8,\"redstoneCount\":10,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":255,\"diamondSize\":8,\"diamondCount\":8,\"diamondMinHeight\":0,\"diamondMaxHeight\":255,\"lapisSize\":7,\"lapisCount\":2,\"lapisCenterHeight\":125,\"lapisSpread\":100}");
           var generatorPreset = JsonConvert.DeserializeObject<WorldGeneratorPreset>("{\"coordinateScale\":175.0,\"heightScale\":75.0,\"lowerLimitScale\":512.0,\"upperLimitScale\":512.0,\"depthNoiseScaleX\":200.0,\"depthNoiseScaleZ\":200.0,\"depthNoiseScaleExponent\":0.5,\"mainNoiseScaleX\":165.0,\"mainNoiseScaleY\":106.61267,\"mainNoiseScaleZ\":165.0,\"baseSize\":8.267606,\"stretchY\":13.387607,\"biomeDepthWeight\":1.2,\"biomeDepthOffset\":0.2,\"biomeScaleWeight\":3.4084506,\"biomeScaleOffset\":0.0,\"seaLevel\":63,\"useCaves\":true,\"useDungeons\":true,\"dungeonChance\":7,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useMonuments\":true,\"useRavines\":true,\"useWaterLakes\":true,\"waterLakeChance\":49,\"useLavaLakes\":true,\"lavaLakeChance\":80,\"useLavaOceans\":false,\"fixedBiome\":-1,\"biomeSize\":8,\"riverSize\":5,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":256,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":256,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisCenterHeight\":16,\"lapisSpread\":16}");
           // GeneratorPreset = JsonConvert.DeserializeObject<WorldGeneratorPreset>(
          //      "{\"useCaves\":true,\"useStrongholds\":true,\"useVillages\":true,\"useMineShafts\":true,\"useTemples\":true,\"useRavines\":true,\"useMonuments\":true,\"useMansions\":true,\"useLavaOceans\":false,\"useWaterLakes\":true,\"useLavaLakes\":true,\"useDungeons\":true,\"fixedBiome\":-3,\"biomeSize\":4,\"seaLevel\":63,\"riverSize\":4,\"waterLakeChance\":4,\"lavaLakeChance\":80,\"dungeonChance\":8,\"dirtSize\":33,\"dirtCount\":10,\"dirtMinHeight\":0,\"dirtMaxHeight\":255,\"gravelSize\":33,\"gravelCount\":8,\"gravelMinHeight\":0,\"gravelMaxHeight\":255,\"graniteSize\":33,\"graniteCount\":10,\"graniteMinHeight\":0,\"graniteMaxHeight\":80,\"dioriteSize\":33,\"dioriteCount\":10,\"dioriteMinHeight\":0,\"dioriteMaxHeight\":80,\"andesiteSize\":33,\"andesiteCount\":10,\"andesiteMinHeight\":0,\"andesiteMaxHeight\":80,\"coalSize\":17,\"coalCount\":20,\"coalMinHeight\":0,\"coalMaxHeight\":128,\"ironSize\":9,\"ironCount\":20,\"ironMinHeight\":0,\"ironMaxHeight\":64,\"goldSize\":9,\"goldCount\":2,\"goldMinHeight\":0,\"goldMaxHeight\":32,\"redstoneSize\":8,\"redstoneCount\":8,\"redstoneMinHeight\":0,\"redstoneMaxHeight\":16,\"diamondSize\":8,\"diamondCount\":1,\"diamondMinHeight\":0,\"diamondMaxHeight\":16,\"lapisSize\":7,\"lapisCount\":1,\"lapisMinHeight\":0,\"lapisMaxHeight\":32,\"coordinateScale\":684,\"heightScale\":684,\"mainNoiseScaleX\":80,\"mainNoiseScaleY\":160,\"mainNoiseScaleZ\":80,\"depthNoiseScaleX\":200,\"depthNoiseScaleZ\":200,\"depthNoiseScaleExponent\":0.5,\"biomeDepthWeight\":1,\"biomeDepthOffset\":0,\"biomeScaleWeight\":1,\"biomeScaleOffset\":1,\"lowerLimitScale\":512,\"upperLimitScale\":512,\"baseSize\":8.5,\"stretchY\":12,\"lapisCenterHeight\":16,\"lapisSpread\":16}");
            WaterLevel = generatorPreset.SeaLevel;

            var seed = "test-world".GetHashCode();
            Seed = seed;
            NoiseProvider = new NoiseProvider(generatorPreset, seed);
        }

        public void Initialize()
        {
            
        }

        /// <inheritdoc />
        public void Initialize(IWorldProvider worldProvider)
        {
            
        }

        public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
        {
            return GenerateChunk(chunkCoordinates).Result;
        }

        private async Task<ChunkColumn> GenerateChunk(ChunkCoordinates chunkCoordinates)
        {
            ChunkColumn chunk = new ChunkColumn();
            chunk.X = chunkCoordinates.X;
            chunk.Z = chunkCoordinates.Z;
            
            Decorators.ChunkDecorator[] chunkDecorators = new ChunkDecorator[]
            {
                new WaterDecorator(),
                //new OreDecorator(),
                new FoliageDecorator(),
            };
            
            foreach (var i in chunkDecorators)
            {
                i.SetSeed(Seed);
            }

            var res = await GenerateNeeded(chunkCoordinates);

            var heightMap = res.heightMap;
            var thresholdMap = res.thresholdMap;
            var biomes = res.biomes;
            
            var blocks = await CreateTerrainShape(heightMap, thresholdMap);
            
            int[] metadata = new int[16 * 16 * 256];
            
            DecorateChunk(chunk.X, chunk.Z, blocks, metadata, heightMap, thresholdMap, biomes, chunkDecorators);

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var height = heightMap[(x << 4) + z];
                    var biome = biomes[(x << 4) + z];

                    chunk.biomeId[(x << 4) + z] = (byte) biome.Id;
                    chunk.height[(x << 4) + z] = (short) height;
                    
                    for (int y = 0; y < 256; y++)
                    {
                        var idx = GetIndex(x, y, z);
                        chunk.SetBlock(x, y, z, blocks[idx]);
                       // chunk.SetMetadata(x,y,z, (byte) metadata[idx]);
                    }
                }
            }

            return chunk;
        }

        public async Task<(float[] baseHeight, float[] heightMap, float[] thresholdMap, BiomeBase[] biomes)> GenerateNeeded(ChunkCoordinates coordinates)
        {
            var baseHeight =
                await GenerateSmoothMap(NoiseProvider.BaseHeightNoise, -1f, 1f, 0f, 128f, coordinates.X, coordinates.Z);

            var biomesTask = CalculateBiomes(baseHeight, coordinates.X, coordinates.Z);
            var thresholdMapTask = GetThresholdMap(coordinates.X, coordinates.Z);

            await Task.WhenAll(biomesTask, thresholdMapTask);

            var biomes = biomesTask.Result;
            var thresholdMap = thresholdMapTask.Result;
            
            var heightMap = await GenerateHeightMap(biomes, coordinates.X, coordinates.Z);

            return (baseHeight, heightMap, thresholdMap, biomes);
        }
        
        private void DecorateChunk(int chunkX, int chunkZ, int[] blocks, int[] metadata, float[] heightMap, float[] thresholdMap, BiomeBase[] biomes,
            ChunkDecorator[] decorators)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var height = heightMap[(x << 4) + z];
                    var biome = biomes[(x << 4) + z];

                    for (int y = 0; y < 256; y++)
                    {
                        var idx = GetIndex(x, y, z);
                        bool isSurface = false;
                        if (y <= height)
                        {
                            if (y < 255 && blocks[idx] == 1 && blocks[GetIndex(x, y + 1, z)] == 0)
                            {
                                isSurface = true;
                            }

                            if (isSurface)
                            {
                                if (y >= WaterLevel)
                                {
                                    blocks[idx] = biome.SurfaceBlock;
                                    metadata[idx] = biome.SurfaceMetadata;
                                    
                                    blocks[GetIndex(x, y - 1, z)] = biome.SoilBlock;
                                    metadata[GetIndex(x, y - 1, z)] = biome.SoilMetadata;
                                }
                            }
                        }

                        for (int i = 0; i < decorators.Length; i++)
                        {
                            decorators[i].Decorate( chunkX, chunkZ, blocks, metadata, biome, thresholdMap, x, y, z, isSurface, y < height - 1);
                        }
                    }
                }
            }
        }

        public async Task<float[]> GenerateSmoothMap(IModule2D noise, float noiseMin, float noiseMax, float rangeStart, float rangeEnd, int chunkX, int chunkZ)
        {
            return await Task.Run(() =>
            {
                int minX = ((chunkX) * 16) ;
                int minZ = ((chunkZ) * 16) ;
                var maxX = ((chunkX + 1) << 4) ;
                var maxZ = ((chunkZ + 1) << 4) ;

                int cx = (chunkX * 16);
                int cz = (chunkZ * 16);

                float q11 = noise.GetValue(minX, minZ);
                float q12 = noise.GetValue(minX, maxZ);

                float q21 = noise.GetValue(maxX, minZ);
                float q22 = noise.GetValue(maxX, maxZ);

                float[] heightMap = new float[16 * 16];

                for (int x = 0; x < 16; x++)
                {
                    float rx = cx + x;

                    for (int z = 0; z < 16; z++)
                    {
                        float rz = cz + z;

                        var baseNoise = MathUtils.BilinearCmr(
                            rx, rz,
                            q11,
                            q12,
                            q21,
                            q22,
                            minX, maxX, minZ, maxZ);

                        heightMap[(x << 4) + z] = MathUtils.ConvertRange(noiseMin, noiseMax, rangeStart, rangeEnd, baseNoise); //WaterLevel + ((128f * baseNoise));
                    }


                }

                return heightMap;
            });
        }
        
        public const float Threshold = 0.2f;
        private async Task<int[]> CreateTerrainShape(float[] heightMap, float[] thresholdMap)
        {
            return await Task.Run(() =>
            {
                int[] blocks = new int[16 * 16 * 256];

                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        var idx = GetIndex(x, z);
                        float stoneHeight = heightMap[idx];

                        var maxY = 0;
                        for (int y = 0; y < stoneHeight && y < 255; y++)
                        {
                            float density = thresholdMap[GetIndex(x, y, z)];

                            if (y < WaterLevel || (density > Threshold))
                            {
                                //   chunk.SetBlock(x, y, z, 1);
                                blocks[GetIndex(x, y, z)] = 1;
                                maxY = y;
                            }
                        }

                        blocks[GetIndex(x, 0, z)] = 7;
                        heightMap[idx] = maxY;
                    }
                }

                return blocks;
            });
        }

        private async Task<float[]> GetThresholdMap(int cx, int cz)
        {
            return await Task.Run(() =>
            {
                cx *= 16;
                cz *= 16;

                float[] thresholdMap = new float[16 * 16 * 256];

                for (int x = 0; x < 16; x++)
                {
                    float rx = cx + x;
                    for (int z = 0; z < 16; z++)
                    {
                        float rz = cz + z;

                        for (int y = 255; y > 0; y--)
                        {
                            thresholdMap[x + ((y + (z << 8)) << 4)] = NoiseProvider.DepthNoise.GetValue(rx, y, rz);
                        }
                    }
                }

                return thresholdMap;
            });
        }

        private async Task<float[]> GenerateHeightMap(BiomeBase[] biomes, int chunkX, int chunkZ)
        {
            return await Task.Run(() =>
            {
                int minX = ((chunkX) * 16) ;
                int minZ = ((chunkZ) * 16) ;
                var maxX = ((chunkX + 1) << 4) ;
                var maxZ = ((chunkZ + 1) << 4) ;

                int cx = (chunkX * 16);
                int cz = (chunkZ * 16);

                /*float q11 = MathUtils.Lerp(biomes[0].MinHeight, biomes[0].MaxHeight,
                    MathUtils.ConvertRange(-1f, 1f, 0f, 1f, NoiseProvider.TerrainNoise.GetValue(minX, minZ)));
                float q12 = MathUtils.Lerp(biomes[15].MinHeight, biomes[15].MaxHeight,
                    MathUtils.ConvertRange(-1f, 1f, 0f, 1f, NoiseProvider.TerrainNoise.GetValue(minX, maxZ)));

                float q21 = MathUtils.Lerp(biomes[240].MinHeight, biomes[240].MaxHeight,
                    MathUtils.ConvertRange(-1f, 1f, 0f, 1f,NoiseProvider.TerrainNoise.GetValue(maxX, minZ)));
                float q22 = MathUtils.Lerp(biomes[255].MinHeight, biomes[255].MaxHeight,
                    MathUtils.ConvertRange(-1f, 1f, 0f, 1f, NoiseProvider.TerrainNoise.GetValue(maxX, maxZ)));*/

                
                float q11 = MathUtils.Lerp(biomes[0].MinHeight, biomes[0].MaxHeight,
                    NoiseProvider.TerrainNoise.GetValue(minX, minZ));
                float q12 = MathUtils.Lerp(biomes[15].MinHeight, biomes[15].MaxHeight,
                    NoiseProvider.TerrainNoise.GetValue(minX, maxZ));

                float q21 = MathUtils.Lerp(biomes[240].MinHeight, biomes[240].MaxHeight,
                    NoiseProvider.TerrainNoise.GetValue(maxX, minZ));
                float q22 = MathUtils.Lerp(biomes[255].MinHeight, biomes[255].MaxHeight,
                    NoiseProvider.TerrainNoise.GetValue(maxX, maxZ));
                
                float[] heightMap = new float[16 * 16];

                for (int x = 0; x < 16; x++)
                {
                    float rx = cx + x;

                    for (int z = 0; z < 16; z++)
                    {
                        float rz = cz + z;

                        var baseNoise = MathUtils.BilinearCmr(
                            rx, rz,
                            q11,
                            q12,
                            q21,
                            q22,
                            minX, maxX, minZ, maxZ);

                        heightMap[(x << 4) + z] = baseNoise; //WaterLevel + ((128f * baseNoise));
                    }


                }

                return heightMap;
            });
        }

        public BiomeBase GetBiome(float x, float z, float height)
        {
           // x /= CoordinateScale;
          //  z /= CoordinateScale;

            var mX = x;// + BiomeModifierX.GetValue(x, z);
            var mZ = z;// + BiomeModifierZ.GetValue(x, z);

            var temp = MathUtils.ConvertRange(-1f, 1f, -1f, 2f, NoiseProvider.TempNoise.GetValue(mX, mZ));// _tempNoise.GetValue(mX, mZ);
            var rain = MathUtils.ConvertRange(-1f, 1f, 0, 1f, NoiseProvider.RainNoise.GetValue(mX, mZ));//MathUtils.ConvertRange(0f, 1f, 0f, 2f, _rainNoise.GetValue(mX, mZ));// _rainNoise.GetValue(mX, mZ);

            var terrainHeight = NoiseProvider.TerrainNoise.GetValue(mX, mZ);
                //     terrainHeight = MathUtils.ConvertRange(-1f, 1f, 0f, 1f, terrainHeight);

               /* return BiomeUtils.Biomes
                    .OrderBy(biome =>
                        MathF.Abs((MathUtils.Lerp(biome.MinHeight, biome.MaxHeight, terrainHeight) - height)))
                    .ThenBy(biome => BiomeUtils.GetSquaredDistance(biome, temp, rain))
                    //.ThenBy(bb => Math.Abs(bb.Temperature - temp)).ThenBy(bb => Math.Abs(bb.Downfall - rain))
                    .FirstOrDefault();*/
               
               return BiomeUtils.Biomes
                   .OrderBy(bb => Math.Abs(bb.Temperature - temp)).ThenBy(bb => Math.Abs(bb.Downfall - rain))
                   .ThenBy(biome => MathF.Abs((MathUtils.Lerp(biome.MinHeight, biome.MaxHeight, terrainHeight) - height)))
                   //.ThenBy(bb => Math.Abs(bb.Temperature - temp)).ThenBy(bb => Math.Abs(bb.Downfall - rain))
                   .FirstOrDefault();
            
            //var upper = _upperLimit.GetValue(mX, mZ);
            //var lower = _lowerLimit.GetValue(mX, mZ);
            
            /*var ordered = BiomeUtils.GetOrderedBiomes(temp, rain, height).Take(3).OrderBy(biome =>
            {
                var avg = MathUtils.Lerp(biome.MinHeight, biome.MaxHeight, MathUtils.ConvertRange(-1f, 1f, -2f, 2f, _terrainNoise.GetValue(mX, mZ)));
                return MathF.Abs((avg - height) * (avg - height));
            }).FirstOrDefault();//.FirstOrDefault(xx => xx.MinHeight >= lower && xx.MaxHeight <= upper);
            return ordered;*/
            //if (temp < -1f) temp = -(temp%1);
           // if (rain < 0) rain = -rain;

           return  BiomeUtils.GetBiome(temp, rain);
        }

        public async Task<BiomeBase[]> CalculateBiomes(float[] baseHeight, int chunkX, int chunkZ)
        {
           // chunkX *= 16;
           //
           // chunkZ *= 16;
            return await Task.Run(() =>
            {
                int minX = (chunkX * 16);
                int minZ = (chunkZ * 16);
                var maxX = ((chunkX + 1) << 4);
                var maxZ = ((chunkZ + 1) << 4);

                
                
                BiomeBase[] rb = new BiomeBase[16 * 16];

                for (int x = 0; x < 16; x++)
                {
                    float rx = MathUtils.Lerp(minX, maxX, (1f / 16f) * x);
                    for (int z = 0; z < 16; z++)
                    {
                        var rz = MathUtils.Lerp(minZ, maxZ, (1f / 16f) * z);
                        var idx = GetIndex(x, z);
                        
                        var biome = GetBiome(rx, rz, baseHeight[idx]);

                      //  var min = MathUtils.ConvertRange(-2f, 2f, 0f, 128f, biome.MinHeight);
                       // var max = MathUtils.ConvertRange(-2f, 2f, 0f, 128f, biome.MaxHeight);
                        
                        rb[idx] = biome;
                    }
                }

                return rb;
            });
        }

        public static int GetIndex(int x, int z)
        {
            return (x << 4) + z;
        }
        
        public static int GetIndex(int x, int y, int z)
        {
            return x + ((y + (z << 8)) << 4);
        }
    }
}