using System;
using Microsoft.Xna.Framework;

namespace MiMap.Viewer.DesktopGL
{
    public class MapRegion
    {
        public readonly int X;
        public readonly int Z;

        public readonly MapChunk[] Chunks;
        public bool IsComplete { get; internal set; }

        public MapRegion(int x, int z)
        {
            X = x;
            Z = z;
            Chunks = new MapChunk[32 * 32];
        }

        public void SetChunk(int cx, int cz, MapChunk chunk)
        {
            Chunks[GetIndex(cx & 31, cz & 31)] = chunk;
        }

        public MapChunk this[int cx, int cz]
        {
            get => Chunks[GetIndex(cx & 31, cz & 31)];
            set => Chunks[GetIndex(cx & 31, cz & 31)] = value;
        }

        private int GetIndex(int x, int z)
        {
            return (x * 32) + z;
        }
    }

    public class MapChunk
    {
        public readonly int X;
        public readonly int Z;

        public readonly int[] Heights;
        public readonly byte[] Biomes;
        public readonly Color[] Colors;

        public MapChunk(int x, int z)
        {
            X = x;
            Z = z;
            Heights = new int[16 * 16];
            Biomes = new byte[16 * 16];
            Colors = new Color[16 * 16];
        }

        public int GetHeight(int x, int z)
        {
            return Heights[GetIndex(x & 15, z & 15)];
        }

        public byte GetBiome(int x, int z)
        {
            return Biomes[GetIndex(x & 15, z & 15)];
        }

        public Color GetColor(int x, int z)
        {
            return Colors[GetIndex(x & 15, z & 15)];
        }

        public void SetHeight(int x, int z, int height)
        {
            var i = GetIndex(x & 15, z & 15);
            Heights[i] = height;
            UpdateColor(i);
        }

        public void SetBiome(int x, int z, byte biome)
        {
            var i = GetIndex(x & 15, z & 15);
            Biomes[i] = biome;
            UpdateColor(i);
        }

        private void UpdateColor(int i)
        {
            var biome = Biomes[i];
            var height = Heights[i];
            var hIntensity = MathHelper.Clamp((height % (255f / 25f)) / 25f, 0f, 1f) / 2;

            var c1 = Globals.BiomeColors[biome];
            var c2 = Color.Black;
            Colors[i] = Color.Lerp(c1, c2, hIntensity);
        }

        private int GetIndex(int x, int z)
        {
            return (x * 16) + z;
        }
    }
}