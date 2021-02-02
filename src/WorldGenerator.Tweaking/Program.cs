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
        private const int Radius     = 256;
        private const int Resolution = 4;
        //private static ConcurrentQueue<ChunkColumn> Finished = new ConcurrentQueue<ChunkColumn>();
        static void Main(string[] args)
        {
            WorldGen = new OverworldGeneratorV2();
            Game = new TestGame(WorldGen, Radius, Resolution);
            //  gen.ApplyBlocks = true;
            
            bool done = false;
          //  ChunkColumn[] generatedChunks = new ChunkColumn[chunks * chunks];
            ConcurrentQueue<ChunkCoordinates> chunkGeneratorQueue = new ConcurrentQueue<ChunkCoordinates>();
            
            long average = 0;
            long min = long.MaxValue;
            long max = long.MinValue;

            int      chunskGenerated = 0;
            Thread[] threads         = new Thread[Environment.ProcessorCount / 2];
            for (int t = 0; t < threads.Length; t++)
            {
                threads[t] = new Thread(() =>
                {
                    Stopwatch timing = new Stopwatch();
                    while (true)
                    {
                        if (chunkGeneratorQueue.TryDequeue(out var coords))
                        {
                            timing.Restart();
                            
                           // ChunkColumn column = WorldGen.GenerateChunkColumn(coords);
                            NoiseData   nd     = new NoiseData();
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
                            lock (Game.Lock)
                            {
                                Game.Chunks.Add(nd);
                            }

                            // generatedChunks[(coords.X * chunks) + coords.Z] = column;
                           // Finished.Enqueue(column);
                            chunskGenerated++;

                            timing.Stop();
                            
                            average += timing.ElapsedMilliseconds;
                            if (timing.ElapsedMilliseconds < min)
                                min = timing.ElapsedMilliseconds;
                            
                            if (timing.ElapsedMilliseconds > max)
                                max = timing.ElapsedMilliseconds;
                        }
                        else
                        {
                            break;
                        }
                    }
                });
            }
            
           // threads[0] = new Thread(() => { GenerateBiomeMap(chunks); });

            for (int z = 0; z < Radius; z+= Resolution)
            {
                for(int x= 0; x < Radius; x+= Resolution)
                {
                    chunkGeneratorQueue.Enqueue(new ChunkCoordinates(x, z));
                }
            }
            
            Stopwatch timer = Stopwatch.StartNew();
            foreach (var thread in threads)
            {
                thread.Start();
            }

           
                Game.Run();

                timer.Stop();
            
            Console.Clear();
            
            Console.WriteLine($"Generating {Radius * Radius} chunks took: {timer.Elapsed}");
            //Console.WriteLine($"Min Height: {gen.MinHeight} Max Height: {gen.MaxHeight}");
        }

        public static OverworldGeneratorV2 WorldGen { get; set; }
    }
}