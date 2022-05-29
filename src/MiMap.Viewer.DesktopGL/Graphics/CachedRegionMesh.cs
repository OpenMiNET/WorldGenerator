using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public class CachedRegionMesh : IDisposable
    {
        // FaceGroupOptimizer
        // FaceGroupUtil
        public Texture2D Texture { get; private set; }

        public Matrix World { get; private set; }
        
        public CachedRegionMesh(GraphicsDevice graphics, MapRegion region)
        {
            var t = new Texture2D(graphics, 1 << 9, 1 << 9);
            var d = new Color[(1 << 9) * (1 << 9)];
            
            int x, y, z;
            byte b;
            Color c;
            foreach (var chunk in region.Chunks)
            {
                for (int cx = 0; cx < 16; cx++)
                for (int cz = 0; cz < 16; cz++)
                {
                    x = (chunk.X << 4) + cx;
                    z = (chunk.Z << 4) + cz;
                    b = chunk.GetBiome(cx, cz);
                    y = chunk.GetHeight(cx, cz);
                    c = chunk.GetColor(cx, cz);

                    d[(((((chunk.X & 31) << 4)) + cx) * (1 << 9)) + (((chunk.Z & 31) << 4) + cz)] = c;
                }
            }
            
            t.SetData(d);
            Texture = t;
            World = Matrix.Identity
                    * Matrix.CreateTranslation((region.X << 9), (region.Z << 9), 0f);
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }
    }
}