using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Graphics;
using MiMap.Viewer.DesktopGL.Primitive;
using MiMap.Viewer.DesktopGL.Utilities;
using MiMap.Viewer.Element.MiMapTiles;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using NLog;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace MiMap.Viewer.DesktopGL
{
    public class Map : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public event EventHandler<Point> ChunkGenerated;

        public event EventHandler<ChunkMesh> ChunkMeshAdded;
        public event EventHandler<ChunkMesh> ChunkMeshUpdated;


        public Dictionary<Vector2i, ChunkMesh> Chunks { get; } = new Dictionary<Vector2i, ChunkMesh>();

        public readonly BiomeRegistry BiomeRegistry = new BiomeRegistry();

        private readonly IWorldGenerator _worldGenerator;
        private GraphicsDevice _graphicsDevice;

        private ConcurrentWorkItemQueue<Vector2i> _workItemQueue;
        private ConcurrentBag<ChunkMesh> _newChunks;

        //    private ConcurrentWorkItemQueue<ChunkMesh> _remeshChunkQueue;
        private ConcurrentBag<ChunkMesh> _updatedChunks;

        public Map(IWorldGenerator worldGenerator)
        {
            _worldGenerator = worldGenerator;

            _workItemQueue = new ConcurrentWorkItemQueue<Vector2i>(GenerateRegion, threadCount: Environment.ProcessorCount-1, trackCompletedTasks: true);
            _newChunks = new ConcurrentBag<ChunkMesh>();

            //  _remeshChunkQueue = new ConcurrentWorkItemQueue<ChunkMesh>(RemeshChunk, threadCount: Environment.ProcessorCount / 2);
            _updatedChunks = new ConcurrentBag<ChunkMesh>();

            ChunkMeshBuilder.InitializeColorMap(BiomeRegistry);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            if (_graphicsDevice != null && Chunks.Count > 0)
            {
                var chunks = Chunks.Values.ToArray();
                foreach (var chunk in chunks)
                {
                    chunk.Reload(graphicsDevice);
                }
            }
        }

        public void StartBackgroundGeneration()
        {
            //   _workItemQueue.Start();
            //   _remeshChunkQueue.Start();
        }

        public void EnqueueRegion(Vector2i regionCoords)
        {
            
        }
        public void EnqueueChunk(Vector2i chunkCoords)
        {
            if (Chunks.ContainsKey(chunkCoords))
                return;
            if (!_workItemQueue.TryEnqueue(chunkCoords))
                return;

            Log.Debug($"Enqueue Chunk: {chunkCoords.X:N2}, {chunkCoords.Z:N2}");
        }

        public void EnqueueRemeshChunk(ChunkMesh chunk)
        {
            //  if (!_remeshChunkQueue.TryEnqueue(chunk))
            //     return;

            //   Log.Debug($"Enqueue Remesh for Chunk: {chunk.X:N2}, {chunk.Z:N2}");
        }

        private void GenerateRegion(Vector2i regionCoords)
        {
            Log.Debug($"Generating Region: {regionCoords.X:N2}, {regionCoords.Z:N2}");
            var csw = Stopwatch.StartNew();
            double t1;

            ChunkCoordinates chunkCoordinates;
            var regionMesh = new ChunkMesh(regionCoords * (1 << 9), 1 << 9);

            int offsetX, offsetZ;
            for (int x = 0; x < 32; x++)
            for (int z = 0; z < 32; z++)
            {
                chunkCoordinates = new ChunkCoordinates((regionCoords.X << 5) + x, (regionCoords.Z << 5) + z);
                using (var chunk = _worldGenerator.GenerateChunkColumn(chunkCoordinates))
                {
                    ExtractMapChunk(ref regionMesh, chunk, x << 4, z << 4);
                }
            }
            
            regionMesh.Recalculate();

            ChunkMeshBuilder.GenerateMesh(this, ref regionMesh);
            _newChunks.Add(regionMesh);
            
            csw.Stop();
            Log.Debug($"Completed Region {regionCoords.X:N2}, {regionCoords.Z:N2} in {csw.ElapsedMilliseconds:N2} ms");
        }

        private void GenerateChunk(Vector2i chunkCoords)
        {
            Log.Debug($"Generating Chunk: {chunkCoords.X:N2}, {chunkCoords.Z:N2}");
            var csw = Stopwatch.StartNew();
            var chunkPosition = new ChunkCoordinates(chunkCoords.X, chunkCoords.Z);
            double t1;
            ChunkMesh mapChunk = new ChunkMesh(chunkPosition);
            using (var chunk = _worldGenerator.GenerateChunkColumn(chunkPosition))
            {
                csw.Stop();
                t1 = csw.ElapsedMilliseconds;
                csw.Restart();
                ExtractMapChunk(ref mapChunk, chunk);

                foreach (var section in chunk)
                    section?.Dispose();
            }

            ChunkMeshBuilder.GenerateMesh(this, ref mapChunk);
            _newChunks.Add(mapChunk);

            csw.Stop();
            Log.Debug($"Completed Chunk {chunkPosition.X:N2}, {chunkPosition.Z:N2} in {t1:N3} ms (generation: {t1:N3} ms, dataExtraction: {csw.ElapsedMilliseconds:N3} ms)");
        }

        private void ExtractMapChunk(ref ChunkMesh chunk, ChunkColumn chunkColumn, int offsetX = 0, int offsetZ = 0)
        {
            int idx, i;
            for (int x = 0; x < 16; x++)
            for (int z = 0; z < 16; z++)
            {
                i = (z * 16) + x;
                
                chunk.Biomes[offsetX + x, offsetZ+z] = chunkColumn.biomeId[i];
                chunk.Heights[offsetX + x, offsetZ+z] = chunkColumn.height[i];
            }

            //chunk.Recalculate();
        }

        private SemaphoreSlim _generateSemaphoreSlim = new SemaphoreSlim(1);

        public void GenerateMissingChunks(Rectangle bounds)
        {
            if (!_generateSemaphoreSlim.Wait(0))
                return;

            new Thread(() =>
            {
                try
                {
                    foreach (var chunkCoords in Spiral.FillRegionFromCenter(bounds))
                    {
                        var cc = new Vector2i(chunkCoords.X, chunkCoords.Y);

                        if (Chunks.TryGetValue(cc, out _))
                            continue;

                        EnqueueChunk(cc);
                    }
                }
                finally
                {
                    _generateSemaphoreSlim.Release();
                }
            }).Start();
        }

        public void GenerateMissingChunks(ChunkCoordinates position, int radius)
        {
            var region = new Rectangle(position.X - radius, position.Z - radius, 2 * radius, 2 * radius);
            GenerateMissingChunks(region);
        }

        private void TryRemeshNeighborChunk(int x, int z)
        {
            if (Chunks.TryGetValue(new Vector2i(x, z), out var chunk))
            {
                if (chunk.HasAllMeshNeighbors) return;

                EnqueueRemeshChunk(chunk);
            }
        }

        private void RemeshNeighborChunks(ChunkCoordinates chunkCoordinates)
        {
            TryRemeshNeighborChunk(chunkCoordinates.X + 1, chunkCoordinates.Z);
            TryRemeshNeighborChunk(chunkCoordinates.X, chunkCoordinates.Z + 1);
            TryRemeshNeighborChunk(chunkCoordinates.X - 1, chunkCoordinates.Z);
            TryRemeshNeighborChunk(chunkCoordinates.X, chunkCoordinates.Z - 1);
        }

        private void RemeshChunk(ChunkMesh chunk)
        {
            if (chunk.HasAllMeshNeighbors) return;

            ChunkMeshBuilder.GenerateMesh(this, ref chunk);
            _updatedChunks.Add(chunk);
        }

        public void Update()
        {
            if (_graphicsDevice != null)
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 5 && _newChunks.TryTake(out var chunk))
                {
                    //_addedchunks.Add(chunk);
                    var p = new Vector2i(chunk.Position.X >> 5, chunk.Position.Z >> 5);
                    if (Chunks.TryAdd(p, chunk))
                    {
                        chunk.Reload(_graphicsDevice);
                        //RemeshNeighborChunks(chunk.ChunkCoordinates);
                        ChunkMeshAdded?.Invoke(this, chunk);
                    }
                }
//                 
// sw.Restart();
//                 while (sw.ElapsedMilliseconds < 8 && _updatedChunks.TryTake(out var chunk))
//                 {
//                     chunk.Reload(_graphicsDevice);
//                     
//                     //if (!chunk.HasAllMeshNeighbors)
//                        // RemeshNeighborChunks(chunk.ChunkCoordinates);
//                     
//                     ChunkMeshUpdated?.Invoke(this, chunk);
//                 }
            }
        }

        public void Dispose()
        {
            _workItemQueue.Dispose();
            //_remeshChunkQueue.Dispose();
        }

        public ChunkMesh GetChunk(Vector2i chunkCoordinates)
        {
            if (Chunks.TryGetValue(chunkCoordinates, out var chunk))
            {
                return chunk;
            }

            return null;
        }

        public bool TryGetChunk(int x, int z, out ChunkMesh chunk)
        {
            return Chunks.TryGetValue(new Vector2i(x, z), out chunk);
        }
    }
}