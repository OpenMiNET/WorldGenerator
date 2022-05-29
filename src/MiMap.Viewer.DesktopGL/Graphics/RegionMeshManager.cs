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
    }

    public class RegionMeshManager : IRegionMeshManager
    {
        private GraphicsDevice _graphics;

        private IDictionary<Point, CachedRegionMesh> _cache = new Dictionary<Point, CachedRegionMesh>();

        public RegionMeshManager(GraphicsDevice graphics)
        {
            _graphics = graphics;
        }

        public CachedRegionMesh CacheRegion(MapRegion region)
        {
            var i = new Point(region.X, region.Z);
            CachedRegionMesh cached;
            if (_cache.TryGetValue(i, out cached)) return cached;
            cached = new CachedRegionMesh(_graphics, region);
            _cache[i] = cached;
            return cached;
        }

        public void Dispose()
        {
            var points = _cache.Keys.ToArray();
            foreach (var p in points)
            {
                if (_cache.Remove(p, out var c))
                {
                    c.Dispose();
                }
            }
        }
    }
}