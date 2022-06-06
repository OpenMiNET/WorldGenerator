using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils.Vectors;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public class CachedChunkMesh : ITile, IDisposable
    {
        // FaceGroupOptimizer
        // FaceGroupUtil
        
        public int X { get; }
        public int Z { get; }
        public ChunkCoordinates ChunkCoordinates { get; }
        public Point Position { get; }
        public Texture2D Texture { get; private set; }

        public Matrix World { get; private set; }
        
        public CachedChunkMesh(GraphicsDevice graphics, MapChunk chunk)
        {
            X = chunk.X;
            Z = chunk.Z;
            ChunkCoordinates = new ChunkCoordinates(X, Z);
            Position = new Point(X << 4, Z << 4);
            
            var t = new Texture2D(graphics, 1 << 4, 1 << 4);
            var d = new Color[(1 << 4) * (1 << 4)];
            
            int x, y, z;
            byte b;
            Color c;
            
            for (int bx = 0; bx < 16; bx++)
            for (int bz = 0; bz < 16; bz++)
            {
                b = chunk.GetBiome(bx, bz);
                y = chunk.GetHeight(bx, bz);
                c = chunk.GetColor(bx, bz);

                SetData(d, bx, bz, c);
            }
            

            // for (int i = 0; i < 16; i++)
            // for (int j = 0; j < 1; j++)
            // {
            //     SetData(d, i, j, Color.Red * 0.5f);
            //     SetData(d, j, i, Color.Blue * 0.5f);
            // }
            
            t.SetData(d);
            Texture = t;
            World = Matrix.CreateWorld(new Vector3((chunk.X << 4), (chunk.Z << 4), 0f), Vector3.Forward, Vector3.Up);
            
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetData(Color[] data, int blockX, int blockZ, Color color)
        {
            data[(blockZ * 16) + blockX] = color;
        }
    }
}