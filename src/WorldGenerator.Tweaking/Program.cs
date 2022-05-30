using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MiNET.Utils.Vectors;
using OpenAPI.WorldGenerator.Generators;

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

            var cancellationToken = new CancellationTokenSource();
            new Thread(() =>
            {
                Stopwatch timer = Stopwatch.StartNew();
                Parallel.ForEach(
                    gen, new ParallelOptions()
                    {
                        CancellationToken = cancellationToken.Token
                    }, coords =>
                    {
                        Stopwatch timing = new Stopwatch();
                        timing.Restart();
                        
                        Game.Add(WorldGen.GenerateChunkColumn(coords));

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
            
            cancellationToken.Cancel();
            //Console.WriteLine($"Min Height: {gen.MinHeight} Max Height: {gen.MaxHeight}");
        }

        public static OverworldGeneratorV2 WorldGen { get; set; }
    }
}