using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibNoise;
using MiNET.Utils;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using Newtonsoft.Json;
using OpenAPI.Utils;
using OpenAPI.WorldGenerator.Generators;
using OpenAPI.WorldGenerator.Utils;
using OpenAPI.WorldGenerator.Utils.Noise;
using Biome = OpenAPI.WorldGenerator.Utils.Biome;
using BiomeUtils = OpenAPI.WorldGenerator.Utils.BiomeUtils;

namespace WorldGenerator.Tweaking
{
    class Program
    {
        private static TestGame Game { get; set; }
        public const int Radius     = 372;
        public const int Resolution = 1;
        //private static ConcurrentQueue<ChunkColumn> Finished = new ConcurrentQueue<ChunkColumn>();
        static void Main(string[] args)
        {
            WorldGen = new OverworldGeneratorV2();
            Game = new TestGame(WorldGen, Radius, Resolution);
            //  gen.ApplyBlocks = true;
            
            bool done = false;
          //  ChunkColumn[] generatedChunks = new ChunkColumn[chunks * chunks];
            long average = 0;
            long min = long.MaxValue;
            long max = long.MinValue;

            int      chunskGenerated = 0;
            // threads[0] = new Thread(() => { GenerateBiomeMap(chunks); });

            List<ChunkCoordinates> gen = new List<ChunkCoordinates>();

            for (int z = 0; z < Radius; z+= Resolution)
            {
                for(int x= 0; x < Radius; x+= Resolution)
                {
                    gen.Add(new ChunkCoordinates(x, z));
                }
            }

            new Thread(() =>
            {
                Stopwatch timer = Stopwatch.StartNew();
                Parallel.ForEach(
                    gen, new ParallelOptions()
                    {
                        
                    }, coords =>
                    {
                        Stopwatch timing = new Stopwatch();
                        timing.Restart();

                        // ChunkColumn column = WorldGen.GenerateChunkColumn(coords);
                        NoiseData nd = new NoiseData();
                        nd.X = coords.X;
                        nd.Z = coords.Z;
                        nd.Chunk = WorldGen.GenerateChunkColumn(coords);

                        /*   for (int x = 0; x < 16; x++)
                            {
                                for (int z = 0; z < 16; z++)
                                {
                                    var index = NoiseMap.GetIndex(x, z);
                                    var temp = WorldGen.TemperatureNoise.GetValue((coords.X * 16) + x, (coords.Z * 16) + z) + 1f;
                                    var rain = MathF.Abs(WorldGen.RainfallNoise.GetValue((coords.X * 16) + x, (coords.Z * 16) + z));
        
                                    nd.Temperature[index] = temp;
                                    nd.Humidity[index] = rain;
                                }
                            }
        */
                        //lock (Game.Lock)
                        //{
                        Game.Add(nd);


                        // generatedChunks[(coords.X * chunks) + coords.Z] = column;
                        // Finished.Enqueue(column);
                        chunskGenerated++;

                        timing.Stop();

                        average += timing.ElapsedMilliseconds;

                        if (timing.ElapsedMilliseconds < min)
                            min = timing.ElapsedMilliseconds;

                        if (timing.ElapsedMilliseconds > max)
                            max = timing.ElapsedMilliseconds;
                    });
                
                timer.Stop();
            
                Console.Clear();
            
                Console.WriteLine($"Generating {Radius * Radius} chunks took: {timer.Elapsed}");
            }).Start();

            Game.Run();
            //Console.WriteLine($"Min Height: {gen.MinHeight} Max Height: {gen.MaxHeight}");
        }

        public static OverworldGeneratorV2 WorldGen { get; set; }
    }
}