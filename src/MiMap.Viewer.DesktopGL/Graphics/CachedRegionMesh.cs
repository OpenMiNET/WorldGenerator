using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public class CachedRegionMesh : IDisposable
    {
        // FaceGroupOptimizer
        // FaceGroupUtil
        
        public int X { get; }
        public int Z { get; }
        public Texture2D Texture { get; private set; }

        public Matrix World { get; private set; }
        
        public CachedRegionMesh(GraphicsDevice graphics, MapRegion region)
        {
            X = region.X;
            Z = region.Z;
            var t = new Texture2D(graphics, 1 << 9, 1 << 9);
            var d = new Color[(1 << 9) * (1 << 9)];
            
            int x, y, z;
            byte b;
            Color c;
            
            for (int cx = 0; cx < 32; cx++)
            for (int cz = 0; cz < 32; cz++)
            {
                var chunk = region[cx, cz];
                for (int bx = 0; bx < 16; bx++)
                for (int bz = 0; bz < 16; bz++)
                {
                    x = (cx << 4) + bx;
                    z = (cz << 4) + bz;
                    b = chunk.GetBiome(bx, bz);
                    y = chunk.GetHeight(bx, bz);
                    c = chunk.GetColor(bx, bz);

//                    d[(((((chunk.Z & 31) << 4)) + cz) * (1 << 9)) + (((chunk.X & 31) << 4) + cx)] = c;
                    SetData(d, x, z, c);
                }
            }

            for (int i = 0; i < 512; i++)
            for (int j = 0; j < 5; j++)
            {
                SetData(d, i, j, Color.Red);
                SetData(d, j, i, Color.Blue);
            }
            
            t.SetData(d);
            Texture = t;
            World = Matrix.CreateWorld(new Vector3((region.X << 9), (region.Z << 9), 0f), Vector3.Forward, Vector3.Up);
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }

        private static void SetData(Color[] data, int blockX, int blockZ, Color color)
        {
            data[(blockZ * 512) + blockX] = color;
        }
    }
}