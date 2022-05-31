﻿using System.Collections.Concurrent;
using System.Diagnostics;
using ElementEngine;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using NLog;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public class ConcurrentWorkItemQueue<T> : IDisposable
    {
        public event EventHandler<T> ItemCompleted; 
        public event EventHandler<T> ItemStarted; 

        private readonly int _threadCount;
        private readonly Action<T> _processWorkItem;
        private object _chunksSync = new object();
        private Queue<T> _pending;
        private HashSet<T> _inProgress;
        private HashSet<T> _completed;
        
        private Thread[] _threads;
        private AutoResetEvent _trigger;
        private bool _running;

        public ConcurrentWorkItemQueue(Action<T> processWorkItem) : this(Environment.ProcessorCount, processWorkItem)
        {
        }

        public ConcurrentWorkItemQueue(int threadCount, Action<T> processWorkItem)
        {
            _threadCount = threadCount;
            _processWorkItem = processWorkItem;
            _pending = new Queue<T>();
            _inProgress = new HashSet<T>();
            _completed = new HashSet<T>();
            _trigger = new AutoResetEvent(false);
            _threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                _threads[i] = new Thread(WorkItemThreadRun)
                {
                    Name = "WorldGenerator",
                    IsBackground = false
                };
                _threads[i].Start();
            }
        }

        public void Start()
        {
            _running = true;
            for (int i = 0; i < _threadCount; i++)
            {
                _threads[i].Start();
            }
        }

        public void Stop()
        {
            _running = false;
            _trigger.Set();
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }
            _trigger?.Dispose();
        }

        public bool TryEnqueue(T item)
        {
            lock (_chunksSync)
            {
                if (_pending.Contains(item) || _inProgress.Contains(item) || _completed.Contains(item)) 
                    return false;
                
                _pending.Enqueue(item);
                return true;
            }
        }

        private bool TryDequeue(int timeout, out T item)
        {
            item = default;
            if (!Monitor.TryEnter(_chunksSync, timeout))
                return false;
            try
            {
                if (_pending.TryDequeue(out item))
                {
                    _inProgress.Add(item);
                    return true;
                }

                return false;
            }
            finally
            {
                Monitor.Exit(_chunksSync);
            }
        }

        private void MarkComplete(T item)
        {
            lock (_chunksSync)
            {
                _inProgress.Remove(item);
                _completed.Add(item);
            }
        }

        public void WorkItemThreadRun()
        {
            while (_running)
            {
                _trigger.WaitOne(1000);
                
                while (_running && TryDequeue(50, out var c))
                {
                    ItemStarted?.Invoke(this, c);
                    
                    _processWorkItem.Invoke(c);
                    
                    MarkComplete(c);
                    ItemCompleted?.Invoke(this, c);
                }
                
            }
        }

        public void ClearQueue()
        {
            lock (_chunksSync)
            {
                _pending.Clear();
            }
        }

        public void Reset()
        {
            lock (_chunksSync)
            {
                _pending.Clear();
                _inProgress.Clear();
                _completed.Clear();
            }
        }

        public void Dispose()
        {
            Reset();
            Stop();
            _trigger?.Dispose();
        }
    }

    public class MiMapTilesWorld : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public IWorldGenerator WorldGenerator { get; }
        public const int BLANK_TILE = -1;

        public Vector2I TileSize { get; set; }
        public Vector2I ChunkSize { get; set; }
        public Dictionary<Vector2I, MiMapTilesWorldChunk> Chunks { get; set; } = new Dictionary<Vector2I, MiMapTilesWorldChunk>();
        public Dictionary<int, TileAnimation> TileAnimations { get; set; } = new Dictionary<int, TileAnimation>();

        private ConcurrentWorkItemQueue<Vector2I> _workItemQueue;
        private ConcurrentBag<MiMapTilesWorldChunk> _newChunks;

        public MiMapTilesWorld(IWorldGenerator worldGenerator)
        {
            WorldGenerator = worldGenerator;
            ChunkSize = new Vector2I(512, 512);
            TileSize = new Vector2I(16, 16);
            _workItemQueue = new ConcurrentWorkItemQueue<Vector2I>(GenerateChunk);
            _newChunks = new ConcurrentBag<MiMapTilesWorldChunk>();
        }

        public void EnqueueChunk(Vector2I chunkCoords)
        {
            if (!_workItemQueue.TryEnqueue(chunkCoords))
                return;

            Log.Info($"Enqueue Chunk: {chunkCoords.X:N2}, {chunkCoords.Y:N2}");
        }

        private void GenerateChunk(Vector2I chunkCoords)
        {
            Log.Info($"Generating Chunk: {chunkCoords.X:N2}, {chunkCoords.Y:N2}");
            var csw = Stopwatch.StartNew();
            var chunkPosition = new ChunkCoordinates(chunkCoords.X, chunkCoords.Y);
            using var chunk = WorldGenerator.GenerateChunkColumn(chunkPosition);
            csw.Stop();
            var t1 = csw.ElapsedMilliseconds;
            csw.Restart();
            var worldChunk = ExtractWorldChunk(chunkCoords, chunk);
            _newChunks.Add(worldChunk);
            csw.Stop();
            Log.Debug($"Completed Chunk {chunkPosition.X:N2}, {chunkPosition.Z:N2} in {t1:N3} ms (generation: {t1:N3} ms, dataExtraction: {csw.ElapsedMilliseconds:N3} ms)");
        }

        private MiMapTilesWorldChunk ExtractWorldChunk(Vector2I coords, ChunkColumn chunkColumn)
        {
            var chunk = new MiMapTilesWorldChunk(coords, this);

            var biomeTiles = new int[chunk.TotalTiles];
            var heightTiles = new int[chunk.TotalTiles];
            // var temperatureTiles = new int[chunk.TotalTiles];
            // var humidityTiles = new int[chunk.TotalTiles];

            int i = 0;
            for (int x = 0; x < 16; x++)
            for (int z = 0; z < 16; z++)
            {
                i = (z << 4) + x;
                
                biomeTiles[i] = (int)chunkColumn.biomeId[i];
                heightTiles[i] = chunkColumn.height[i];
            }
            
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Biome, biomeTiles);
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Height, heightTiles);
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Humidity, biomeTiles);
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Temperature, biomeTiles);

            return chunk;
        }

        public void Update()
        {
            while (_newChunks.TryTake(out var chunk))
            {
                Chunks.Add(chunk.Position, chunk);
            }
        }

        public void ClearTiles()
        {
            foreach (var (_, chunk) in Chunks)
                chunk.ClearTiles();
        }

        public void ResetTiles()
        {
            foreach (var (_, chunk) in Chunks)
                chunk.ResetTiles();
        }

        public void Dispose()
        {
            _workItemQueue.Dispose();
        }
    } // EndlessTilesWorld
}