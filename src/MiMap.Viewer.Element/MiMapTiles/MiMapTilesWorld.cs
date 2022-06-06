using System.Collections.Concurrent;
using System.Diagnostics;
using ElementEngine;
using MiMap.Viewer.DesktopGL.Utilities;
using MiMap.Viewer.Element.Graphics;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using NLog;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public class MiMapTilesWorld : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public event EventHandler<MiMapTilesWorldChunk> ChunkAdded;
        public IWorldGenerator WorldGenerator { get; }
        public const int BLANK_TILE = -1;

        public Vector2I TileSize { get; set; }
        public Vector2I ChunkSize { get; set; }
        public Dictionary<Vector2I, MiMapTilesWorldChunk> Chunks { get; set; } = new Dictionary<Vector2I, MiMapTilesWorldChunk>();

        private ConcurrentWorkItemQueue<Vector2I> _workItemQueue;
        private ConcurrentBag<MiMapTilesWorldChunk> _newChunks;

        public MiMapTilesWorld(IWorldGenerator worldGenerator)
        {
            WorldGenerator = worldGenerator;
            ChunkSize = new Vector2I(16, 16);
            TileSize = new Vector2I(1, 1);
            _workItemQueue = new ConcurrentWorkItemQueue<Vector2I>(GenerateChunk);
            _newChunks = new ConcurrentBag<MiMapTilesWorldChunk>();
        }

        public void StartBackgroundGeneration()
        {
            _workItemQueue.Start();
        }

        public void EnqueueChunk(Vector2I chunkCoords)
        {
            if (!_workItemQueue.TryEnqueue(chunkCoords))
                return;

            Logging.Information($"Enqueue Chunk: {chunkCoords.X:N2}, {chunkCoords.Y:N2}");
        }

        private void GenerateChunk(Vector2I chunkCoords)
        {
            Logging.Information($"Generating Chunk: {chunkCoords.X:N2}, {chunkCoords.Y:N2}");
            var csw = Stopwatch.StartNew();
            var chunkPosition = new ChunkCoordinates(chunkCoords.X, chunkCoords.Y);
            using var chunk = WorldGenerator.GenerateChunkColumn(chunkPosition);
            csw.Stop();
            var t1 = csw.ElapsedMilliseconds;
            csw.Restart();
            var worldChunk = ExtractWorldChunk(chunkCoords, chunk);
            _newChunks.Add(worldChunk);
            csw.Stop();
            Logging.Debug($"Completed Chunk {chunkPosition.X:N2}, {chunkPosition.Z:N2} in {t1:N3} ms (generation: {t1:N3} ms, dataExtraction: {csw.ElapsedMilliseconds:N3} ms)");
        }

        private MiMapTilesWorldChunk ExtractWorldChunk(Vector2I coords, ChunkColumn chunkColumn)
        {
            var chunk = new MiMapTilesWorldChunk(coords, this);

            var biomeTiles = new int[chunk.TotalTiles];
            var heightTiles = new int[chunk.TotalTiles];
            var temperatureTiles = new int[chunk.TotalTiles];
            var downfallTiles = new int[chunk.TotalTiles];

            int i = 0;
            byte b = 0;
            var tw = BiomeTileTexture.Width;
            for (int x = 0; x < 16; x++)
            for (int z = 0; z < 16; z++)
            {
                i = (z << 4) + x;
                b = chunkColumn.biomeId[i];

                biomeTiles[i] = b;
                downfallTiles[i] = tw + b;
                temperatureTiles[i] = (2 * tw) + b;
                heightTiles[i] = (3 * tw) + chunkColumn.height[i];
            }

            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Biome, biomeTiles);
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Downfall, downfallTiles);
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Temperature, temperatureTiles);
            chunk.UpdateLayerTiles(MiMapTilesWorldLayerType.Height, heightTiles);

            return chunk;
        }

        public void GenerateMissingChunks(Rectangle bounds)
        {
            foreach (var chunkCoords in Spiral.FillRegionFromCenter(bounds))
            {
                EnqueueChunk(chunkCoords);
            }
        }

        public void GenerateMissingChunks(Vector2I position, int radius)
        {
            var region = new Rectangle(position.X - radius, position.Y - radius, 2 * radius, 2 * radius);
            GenerateMissingChunks(region);
        }

        public void Update()
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 10 && _newChunks.TryTake(out var chunk))
            {
                Chunks.Add(chunk.Position, chunk);
                ChunkAdded?.Invoke(this, chunk);
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