using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public interface IRegionMeshManager : IDisposable
    {
        CachedRegionMesh CacheRegion(MapRegion region);
        CachedChunkMesh CacheChunk(MapChunk chunk);
    }

    public class RegionMeshManager : IRegionMeshManager
    {
        private GraphicsDevice _graphics;

        private IDictionary<Point, CachedRegionMesh> _regionCache = new Dictionary<Point, CachedRegionMesh>();
        private IDictionary<Point, CachedChunkMesh> _chunkCache = new Dictionary<Point, CachedChunkMesh>();

        public RegionMeshManager(GraphicsDevice graphics)
        {
            _graphics = graphics;
        }

        public CachedRegionMesh CacheRegion(MapRegion region)
        {
            var i = new Point(region.X, region.Z);
            CachedRegionMesh cached;
            if (_regionCache.TryGetValue(i, out cached)) return cached;
            cached = new CachedRegionMesh(_graphics, region);
            _regionCache[i] = cached;
            return cached;
        }

        public CachedChunkMesh CacheChunk(MapChunk chunk)
        {
            var i = new Point(chunk.X, chunk.Z);
            CachedChunkMesh cached;
            if (_chunkCache.TryGetValue(i, out cached)) return cached;
            cached = new CachedChunkMesh(_graphics, chunk);
            _chunkCache[i] = cached;
            return cached;
        }

        public void Dispose()
        {
            var points = _regionCache.Keys.ToArray();
            foreach (var p in points)
            {
                if (_regionCache.Remove(p, out var c))
                {
                    c.Dispose();
                }
            }
            points = _chunkCache.Keys.ToArray();
            foreach (var p in points)
            {
                if (_chunkCache.Remove(p, out var c))
                {
                    c.Dispose();
                }
            }
        }
    }
}