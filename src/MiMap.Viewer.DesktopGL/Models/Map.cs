using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using NLog;

namespace MiMap.Viewer.DesktopGL
{
    public class Map : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IWorldGenerator _worldGenerator;
        public Dictionary<Point, MapRegion> Regions { get; }

        public event EventHandler<Point> RegionGenerated;
        public event EventHandler<Point> ChunkGenerated;

        private Thread _thread;
        private ConcurrentQueue<Point> _regionsToGenerate;
        private AutoResetEvent _trigger;
        private bool _running;

        public Map(IWorldGenerator worldGenerator)
        {
            _worldGenerator = worldGenerator;
            Regions = new Dictionary<Point, MapRegion>();
            _regionsToGenerate = new ConcurrentQueue<Point>();
            _trigger = new AutoResetEvent(false);
            _thread = new Thread(Run)
            {
                Name = "WorldGenerator",
                IsBackground = false
            };
            _thread.Start();
        }

        public void Run()
        {
            _running = true;

            while (_running)
            {
                if (_trigger.WaitOne(10000))
                {
                    while (_regionsToGenerate.TryPeek(out var r))
                    {
                        if (Regions.ContainsKey(r))
                        {
                            _regionsToGenerate.TryDequeue(out _);
                            continue;
                        }

                        Log.Info($"Generating Region: {r.X:000}, {r.Y:000}");
                        var sw = Stopwatch.StartNew();
                        Regions[r] = new MapRegion(r.X, r.Y);
                        _regionsToGenerate.TryDequeue(out _);
                        Parallel.For(0, 32 * 32, new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = Environment.ProcessorCount / 2,
                                TaskScheduler = TaskScheduler.Default
                            },
                            (i) =>
                            {
                                var cx = (int)Math.Floor(i / 32f);
                                var cz = i % 32;
                                var chunkPosition = new ChunkCoordinates((r.X << 5) + cx, (r.Y << 5) + cz);
                                var csw = Stopwatch.StartNew();
                                using var chunk = _worldGenerator.GenerateChunkColumn(chunkPosition);
                                csw.Stop();
                                var t1 = csw.ElapsedMilliseconds;
                                csw.Restart();
                                Regions[r][cx, cz] = ExtractChunkData(chunk);
                                csw.Stop();
                                //Log.Info($"Generated Chunk: {chunkPosition.X:000}, {chunkPosition.Z:000} in {t1:F2}ms (extract data in {csw.ElapsedMilliseconds:F2}ms)");
                                ChunkGenerated?.Invoke(this, new Point(chunkPosition.X, chunkPosition.Z));
                            });
                        sw.Stop();
                        Log.Info($"Generated Region: {r.X:000}, {r.Y:000} in {sw.ElapsedMilliseconds:F2}ms");
                        Regions[r].IsComplete = true;
                        RegionGenerated?.Invoke(this, r);
                    }
                }
            }
        }

        private void EnqueueRegion(Point regionCoords)
        {
            if (Regions.ContainsKey(regionCoords) || _regionsToGenerate.Contains(regionCoords))
                return;

            Log.Info($"Enqueue Region: {regionCoords.X:000}, {regionCoords.Y:000}");
            _regionsToGenerate.Enqueue(regionCoords);
            _trigger.Set();
        }

        private MapChunk ExtractChunkData(ChunkColumn chunk)
        {
            var mapChunk = new MapChunk(chunk.X, chunk.Z);
            for (int x = 0; x < 16; x++)
            for (int z = 0; z < 16; z++)
            {
                mapChunk.SetHeight(x, z, chunk.GetHeight(x, z));
                mapChunk.SetBiome(x, z, chunk.GetBiome(x, z));
            }

            return mapChunk;
        }

        public MapRegion GetRegion(Point regionPosition)
        {
            return Regions[regionPosition];
        }

        public IEnumerable<MapRegion> GetRegions()
        {
            var v = Regions.Values.ToArray();
            foreach (var region in v)
            {
                yield return region;
            }
        }
        
        public IEnumerable<MapRegion> GetRegions(Rectangle blockBounds)
        {
            var regionMin = new Point((blockBounds.X >> 9) - 1, (blockBounds.Y >> 9) - 1);
            var regionMax = new Point(((blockBounds.X + blockBounds.Width) >> 9) + 1, ((blockBounds.Y + blockBounds.Height) >> 9) + 1);

            for (int x = regionMin.X; x <= regionMax.X; x++)
            for (int z = regionMin.Y; z <= regionMax.Y; z++)
            {
                var p = new Point(x, z);
                if (Regions.TryGetValue(p, out var region))
                {
                    yield return region;
                }
                else
                {
                    EnqueueRegion(p);
                }
            }
        }

        public void Dispose()
        {
            _running = false;
            _trigger.Set();
            _thread.Join();
            _trigger?.Dispose();
        }
    }
}