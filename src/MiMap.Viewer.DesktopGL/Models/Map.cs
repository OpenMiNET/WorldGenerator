using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using g3;
using Microsoft.Xna.Framework;
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

        private readonly IWorldGenerator _worldGenerator;
        public event EventHandler<ChunkMesh> ChunkAdded;
        public Dictionary<ChunkCoordinates, ChunkMesh> Chunks { get; } = new Dictionary<ChunkCoordinates, ChunkMesh>();
        public Dictionary<Point, MapRegion> Regions { get; } = new Dictionary<Point, MapRegion>();
        public event EventHandler<Point> RegionGenerated;
        public event EventHandler<Point> ChunkGenerated;


        public readonly BiomeRegistry BiomeRegistry = new BiomeRegistry();

        private ConcurrentWorkItemQueue<ChunkCoordinates> _workItemQueue;        
        private ConcurrentBag<ChunkMesh> _newChunks;

        public Map(IWorldGenerator worldGenerator)
        {
            _worldGenerator = worldGenerator;
            _workItemQueue = new ConcurrentWorkItemQueue<ChunkCoordinates>(GenerateChunk);
            _newChunks = new ConcurrentBag<ChunkMesh>();
            
            ChunkMeshBuilder.InitializeColorMap(BiomeRegistry);
        }
        
        public void StartBackgroundGeneration()
        {
            _workItemQueue.Start();
        }
        
        public void EnqueueChunk(ChunkCoordinates chunkCoords)
        {
            if (!_workItemQueue.TryEnqueue(chunkCoords))
                return;

            Log.Debug($"Enqueue Chunk: {chunkCoords.X:N2}, {chunkCoords.Z:N2}");
        }

        private void GenerateChunk(ChunkCoordinates chunkCoords)
        {
            Log.Debug($"Generating Chunk: {chunkCoords.X:N2}, {chunkCoords.Z:N2}");
            var csw = Stopwatch.StartNew();
            var chunkPosition = new ChunkCoordinates(chunkCoords.X, chunkCoords.Z);
            using var chunk = _worldGenerator.GenerateChunkColumn(chunkPosition);
            csw.Stop();
            var t1 = csw.ElapsedMilliseconds;
            csw.Restart();
            var worldChunk = ExtractWorldChunk(chunkCoords, chunk);
            _newChunks.Add(worldChunk);
            csw.Stop();
            Log.Debug($"Completed Chunk {chunkPosition.X:N2}, {chunkPosition.Z:N2} in {t1:N3} ms (generation: {t1:N3} ms, dataExtraction: {csw.ElapsedMilliseconds:N3} ms)");
        }

        private ChunkMesh ExtractWorldChunk(ChunkCoordinates coords, ChunkColumn chunkColumn)
        {
            var chunk = ChunkMeshBuilder.GenerateMesh(MiMapViewer.Instance.GraphicsDevice, chunkColumn);
            return chunk;
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

        public void Update()
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 10 && _newChunks.TryTake(out var chunk))
            {
                Chunks.Add(chunk.ChunkCoordinates, chunk);
                ChunkAdded?.Invoke(this, chunk);
            }
        }

        public void Dispose()
        {
            _workItemQueue.Dispose();
        }

        public ChunkMesh GetChunk(ChunkCoordinates chunkCoordinates)
        {
            if (Chunks.TryGetValue(chunkCoordinates, out var chunk))
            {
                return chunk;
            }

            return null;
        }
    }
}