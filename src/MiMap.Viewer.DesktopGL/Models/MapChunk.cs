using System;
using Microsoft.Xna.Framework;
using MiNET.Utils.Vectors;
using OpenAPI.WorldGenerator.Utils;

namespace MiMap.Viewer.DesktopGL
{
    public class MapChunk
    {
        public readonly int X;
        public readonly int Z;
        public ChunkCoordinates ChunkCoordinates { get; }

        public readonly int[] Heights;
        public readonly byte[] Biomes;

        public MapChunk(ChunkCoordinates chunkCoordinates)
        {
            ChunkCoordinates = chunkCoordinates;
            
            X = chunkCoordinates.X;
            Z = chunkCoordinates.Z;
            Heights = new int[16 * 16];
            Biomes = new byte[16 * 16];
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
            return ((x & 0x0F) * 16) + (z & 0x0F);
        }
    }
}