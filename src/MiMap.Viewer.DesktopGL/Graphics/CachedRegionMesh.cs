using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public interface ITile
    {
        int X { get; }
        int Z { get; }
        Point Position { get; }
        Texture2D Texture { get; }
        Matrix World { get; }
    }
    public class CachedRegionMesh : ITile, IDisposable
    {
        // FaceGroupOptimizer
        // FaceGroupUtil
        
        public int X { get; }
        public int Z { get; }
        public Point Position { get; }
        public Texture2D Texture { get; }

        public Matrix World { get; }

        public CachedRegionMesh(GraphicsDevice graphics, int x, int z)
        {
            X = x;
            Z = z;
            Position = new Point(X << 9, Z << 9);
            var t = new Texture2D(graphics, 1 << 9, 1 << 9);
            
            Texture = t;
            World = Matrix.CreateWorld(new Vector3(Position.X, Position.Y, 0f), Vector3.Forward, Vector3.Up);
        }
        
        public CachedRegionMesh(GraphicsDevice graphics, MapRegion region) : this(graphics, region.X, region.Z)
        {
            var d = new Color[(1 << 9) * (1 << 9)];
            
            int x, y, z;
            byte b;
            Color c;
            
            for (int cx = 0; cx < 32; cx++)
            for (int cz = 0; cz < 32; cz++)
            {
                var chunk = region[cx, cz];
                if (chunk != default)
                {
                    for (int bx = 0; bx < 16; bx++)
                    for (int bz = 0; bz < 16; bz++)
                    {
                        x = (cx << 4) + bx;
                        z = (cz << 4) + bz;
                        b = chunk.GetBiome(bx, bz);
                        y = chunk.GetHeight(bx, bz);
                        c = chunk.GetColor(bx, bz);

                        SetData(d, x, z, c);
                    }
                }
            }

            for (int i = 0; i < 512; i++)
            for (int j = 0; j < 5; j++)
            {
                SetData(d, i, j, Color.Red);
                SetData(d, j, i, Color.Blue);
            }
            
            Texture.SetData(d);
        }

        public void UpdateChunk(MapChunk chunk)
        {
            if (chunk == default) 
                return;
            
            var cx = chunk.X & 0x1F;
            var cz = chunk.Z & 0x1F;
            
            int x, y, z;
            byte b;
            Color c;
            var d = new Color[(1 << 4) * (1 << 4)];

            for (int bx = 0; bx < 16; bx++)
            for (int bz = 0; bz < 16; bz++)
            {
                b = chunk.GetBiome(bx, bz);
                y = chunk.GetHeight(bx, bz);
                c = chunk.GetColor(bx, bz);

                d[(bz * 16) + bx] = c;
            }

            var r = new Rectangle(cx << 4, cz << 4, 16, 16);
            Texture.SetData(0, r, d, 0, 16*16);
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetData(Color[] data, int blockX, int blockZ, Color color)
        {
            data[(blockZ * 512) + blockX] = color;
        }
    }
}