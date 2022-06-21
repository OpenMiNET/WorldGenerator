using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using g3;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Graphics;
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
        
        
        public Dictionary<ChunkCoordinates, ChunkMesh> Chunks { get; } = new Dictionary<ChunkCoordinates, ChunkMesh>();
        
        public readonly BiomeRegistry BiomeRegistry = new BiomeRegistry();

        private readonly IWorldGenerator _worldGenerator;
        private GraphicsDevice _graphicsDevice;

        private ConcurrentWorkItemQueue<ChunkCoordinates> _workItemQueue;        
        private ConcurrentBag<ChunkMesh> _newChunks;
        
        private ConcurrentWorkItemQueue<ChunkMesh> _remeshChunkQueue;
        private ConcurrentBag<ChunkMesh> _updatedChunks;

        public Map(IWorldGenerator worldGenerator)
        {
            _worldGenerator = worldGenerator;
            
            _workItemQueue = new ConcurrentWorkItemQueue<ChunkCoordinates>(GenerateChunk, threadCount: Environment.ProcessorCount -1, trackCompletedTasks: true);
            _newChunks = new ConcurrentBag<ChunkMesh>();
            
            _remeshChunkQueue = new ConcurrentWorkItemQueue<ChunkMesh>(RemeshChunk, threadCount: 1);
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
            _workItemQueue.Start();
            _remeshChunkQueue.Start();
        }
        
        public void EnqueueChunk(ChunkCoordinates chunkCoords)
        {
            if (!_workItemQueue.TryEnqueue(chunkCoords))
                return;

            Log.Debug($"Enqueue Chunk: {chunkCoords.X:N2}, {chunkCoords.Z:N2}");
        }
        public void EnqueueRemeshChunk(ChunkMesh chunk)
        {
            if (!_remeshChunkQueue.TryEnqueue(chunk))
                return;

            Log.Debug($"Enqueue Remesh for Chunk: {chunk.X:N2}, {chunk.Z:N2}");
        }
        private void GenerateChunk(ChunkCoordinates chunkCoords)
        {
            Log.Debug($"Generating Chunk: {chunkCoords.X:N2}, {chunkCoords.Z:N2}");
            var csw = Stopwatch.StartNew();
            var chunkPosition = new ChunkCoordinates(chunkCoords.X, chunkCoords.Z);
            double t1;
            ChunkMesh mapChunk = new ChunkMesh(chunkCoords);
            using (var chunk = _worldGenerator.GenerateChunkColumn(chunkPosition))
            {
                csw.Stop(); t1 = csw.ElapsedMilliseconds; csw.Restart();
                ExtractMapChunk(ref mapChunk, chunk);
            }

            ChunkMeshBuilder.GenerateMesh(this, ref mapChunk);
            _newChunks.Add(mapChunk);
            
            csw.Stop();
            Log.Debug($"Completed Chunk {chunkPosition.X:N2}, {chunkPosition.Z:N2} in {t1:N3} ms (generation: {t1:N3} ms, dataExtraction: {csw.ElapsedMilliseconds:N3} ms)");
        }

        private void ExtractMapChunk(ref ChunkMesh chunk, ChunkColumn chunkColumn)
        {
            for (int i = 0; i < 256; i++)
            {
                chunk.Biomes[i] = chunkColumn.biomeId[i];
                chunk.Heights[i] = chunkColumn.height[i];
            }
            
        }

        public void GenerateMissingChunks(Rectangle bounds)
        {
            foreach (var chunkCoords in Spiral.FillRegionFromCenter(bounds))
            {
                EnqueueChunk(new ChunkCoordinates(chunkCoords.X, chunkCoords.Y));
            }
        }

        public void GenerateMissingChunks(ChunkCoordinates position, int radius)
        {
            var region = new Rectangle(position.X - radius, position.Z - radius, 2 * radius, 2 * radius);
            GenerateMissingChunks(region);
        }

        private void TryRemeshNeighborChunk(int x, int z)
        {
            if (Chunks.TryGetValue(new ChunkCoordinates(x, z), out var chunk))
            {
                if(chunk.HasAllMeshNeighbors) return;
                
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
            if(chunk.HasAllMeshNeighbors) return;
            
            ChunkMeshBuilder.GenerateMesh(this, ref chunk);
            _updatedChunks.Add(chunk);
        }

        private List<ChunkMesh> _addedchunks = new List<ChunkMesh>();
        public void Update()
        {
            if (_graphicsDevice != null)
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 8 && _newChunks.TryTake(out var chunk))
                {
                    _addedchunks.Add(chunk);
                    chunk.Reload(_graphicsDevice);
                    Chunks.Add(chunk.ChunkCoordinates, chunk);
                }

                foreach (var chunk in _addedchunks)
                {
                    RemeshNeighborChunks(chunk.ChunkCoordinates);
                    ChunkMeshAdded?.Invoke(this, chunk);
                }
                _addedchunks.Clear();

                while (sw.ElapsedMilliseconds < 8 && _updatedChunks.TryTake(out var chunk))
                {
                    chunk.Reload(_graphicsDevice);
                    ChunkMeshUpdated?.Invoke(this, chunk);
                }
            }
        }

        public void Dispose()
        {
            _workItemQueue.Dispose();
            _remeshChunkQueue.Dispose();
        }

        public ChunkMesh GetChunk(ChunkCoordinates chunkCoordinates)
        {
            if (Chunks.TryGetValue(chunkCoordinates, out var chunk))
            {
                return chunk;
            }

            return null;
        }

        public bool TryGetChunk(int x, int z, out ChunkMesh chunk)
        {
            return Chunks.TryGetValue(new ChunkCoordinates(x, z), out chunk);
        }
    }
}