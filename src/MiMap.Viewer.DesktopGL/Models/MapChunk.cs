using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MiMap.Viewer.DesktopGL.Models;
using MiMap.Viewer.DesktopGL.Primitive;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Utils;

namespace MiMap.Viewer.DesktopGL
{
    public class MapChunk : IMapChunkData
    {
        public readonly int X;
        public readonly int Z;

        public readonly int[] Heights;
        public readonly byte[] Biomes;
        
        public Vector2i Position { get; }
        public int MaxHeight { get; private set; }

        public Vector3i ChunkSize { get; private set; } = new Vector3i(16, ChunkColumn.WorldHeight, 16);

        public MapChunk(Vector2i position, int sizeXZ = 16)
        {
            Position = position;
            
            X = position.X;
            Z = position.Z;
            ChunkSize = new Vector3i(sizeXZ, ChunkColumn.WorldHeight, sizeXZ);
            Heights = new int[sizeXZ * sizeXZ];
            Biomes = new byte[sizeXZ * sizeXZ];
        }

        public int GetHeight(int x, int z)
        {
            return Heights[GetIndex(x, z)];
        }

        public byte GetBiome(int x, int z)
        {
            return Biomes[GetIndex(x, z)];
        }

        public void SetHeight(int x, int z, int height)
        {
            var i = GetIndex(x, z);
            Heights[i] = height;
        }

        public void SetBiome(int x, int z, byte biome)
        {
            var i = GetIndex(x, z);
            Biomes[i] = biome;
        }

        private int GetIndex(int x, int z)
        {
            x = x < 0 ? Math.Abs(-ChunkSize.X + (x % ChunkSize.X)) : x % ChunkSize.X;
            z = z < 0 ? Math.Abs(-ChunkSize.Z + (z % ChunkSize.Z)) : z % ChunkSize.Z;
            return (x * ChunkSize.Z) + z;
        }


        public int? this[int x, int y, int z]
        {
            get
            {
                var idx = GetIndex(x, z);
                if (idx >= Heights.Length || idx < 0) 
                    return null;
                if (y > Heights[idx]) 
                    return null;

                return Biomes[idx];
            }
        }

        public void Recalculate()
        {
            MaxHeight = Heights.Max();
            ChunkSize = new Vector3i(ChunkSize.X, MaxHeight, ChunkSize.Z);
        }
    }
}