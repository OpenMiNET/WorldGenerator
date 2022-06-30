using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MiNET.Utils.Vectors;
using OpenAPI.WorldGenerator.Generators;
using OpenMiNET.Noise;
using OpenMiNET.Noise.Modules;
using OpenMiNET.Noise.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WorldGenerator.Tweaking
{
    class Program
    {
        private static Game Game { get; set; }
        public const int Radius     = 512;
        public const int Resolution = 1;

        static void Main(string[] args)
        {
            WorldGen = new OverworldGeneratorV2();
            
          //  NoisePreviewGame previewGame = new NoisePreviewGame(WorldGen, Radius, Resolution);
          //  previewGame.Run();
            
          // RunNoiseTest();
        //  ShowNoisePreview();
            RunPreviewer();
            //Console.WriteLine($"Min Height: {gen.MinHeight} Max Height: {gen.MaxHeight}");
        }

        private static void ShowNoisePreview()
        {
            NoisePreviewGame previewGame = new NoisePreviewGame(WorldGen, Radius, Resolution); 
            previewGame.Run();
        }

        private static void RunNoiseTest()
        {
            var seed = 124;
            var biomeScale = 4 * 16f;

            INoiseModule temperatureNoise = new VoronoiNoseModule(new AverageSelectorModule(new SimplexPerlin(seed ^ 3, NoiseQuality.Fast), new SimplexPerlin(seed ^ 64, NoiseQuality.Fast)))
            {
                Distance = true,
                Frequency = 0.0325644f,
                Displacement = 2f
            };

            temperatureNoise = new ScaledNoiseModule(temperatureNoise)
            {
                ScaleX = 1f / biomeScale, ScaleY = 1f / biomeScale, ScaleZ = 1f / biomeScale
            };
            
            Image<Rgba32> output = new Image<Rgba32>(Radius, Radius);
            for (int x = 0; x < Radius; x++)
            {
                for (int z = 0; z < Radius; z++)
                {
                    var temp = MathF.Abs(temperatureNoise.GetValue(x * 16f, z* 16f));
                    //  var rain = WorldGen.RainfallNoise.GetValue(x* 16f, z* 16f);

                    output[x, z] = new Rgba32((1f / 2f) * temp, 0f, 0f);
                }
            }
            output.SaveAsPng("test.png");

            return;
        }

        private static void RunPreviewer()
        {
            var game = new TestGame(WorldGen, Radius, Resolution);
            Game = game;
            
            bool done = false;

            long average = 0;
            long min = long.MaxValue;
            long max = long.MinValue;

            int chunskGenerated = 0;

            List<ChunkCoordinates> gen = new List<ChunkCoordinates>();


            for (int z = 0; z < Radius; z++)
            {
                for (int x = 0; x < Radius; x++)
                {
                    var cc = new ChunkCoordinates((x), (z));
                    gen.Add(cc);
                }
            }


            var cancellationToken = new CancellationTokenSource();

            new Thread(
                () =>
                {
                    int total = gen.Count;
                    Stopwatch timer = Stopwatch.StartNew();

                    Parallel.ForEach(
                        gen, new ParallelOptions() {CancellationToken = cancellationToken.Token}, coords =>
                        {
                            try
                            {
                                Stopwatch timing = new Stopwatch();
                                timing.Restart();

                                game.Add(WorldGen.GenerateChunkColumn(coords));

                                timing.Stop();

                                average += timing.ElapsedMilliseconds;

                                if (timing.ElapsedMilliseconds < min)
                                    min = timing.ElapsedMilliseconds;

                                if (timing.ElapsedMilliseconds > max)
                                    max = timing.ElapsedMilliseconds;

                                if (Interlocked.Increment(ref chunskGenerated) % 500 == 0)
                                {
                                    float progress = (1f / total) * chunskGenerated;
                                    Console.WriteLine($"[{progress:P000}] Generated {chunskGenerated} of {total} chunks. Average={(average / chunskGenerated):F2}ms");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ehhh: {ex}");
                            }
                        });

                    timer.Stop();

                    Console.Clear();

                    Console.WriteLine($"Generating {chunskGenerated} chunks took: {timer.Elapsed}");
                }).Start();

            Game.Run();

            cancellationToken.Cancel();
        }
        
        public static OverworldGeneratorV2 WorldGen { get; set; }
    }
}