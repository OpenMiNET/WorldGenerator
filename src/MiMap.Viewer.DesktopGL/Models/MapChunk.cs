using System;
using System.Collections;
using System.Collections.Generic;
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
        public class BlockData<T> : IReadOnlyCollection<T>
        {
            private readonly int _size;
            private readonly T[] _data;

            internal BlockData(int size)
            {
                _size = size;
                _data = new T[size * size];
            }

            public T this[int idx]
            {
                get => _data[idx];
                set => _data[idx] = value;
            }
            
            public T this[int x, int z]
            {
                get => _data[GetIndex(x, z)];
                set => _data[GetIndex(x, z)] = value;
            }
            
            private int GetIndex(int x, int z)
            {
                return ((z & (_size - 1)) * _size) + (x & (_size - 1));
            }

            public int Count => _data.Length;
            public int Length => _data.Length;
            
            public IEnumerator<T> GetEnumerator()
            {
                return ((ICollection<T>) _data).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _data.GetEnumerator();
            }
        }
        
        public readonly int X;
        public readonly int Z;

        public readonly BlockData<int> Heights;
        public readonly BlockData<byte> Biomes;

        public Vector2i Position { get; }
        public int MaxHeight { get; private set; }

        public Vector3i ChunkSize { get; private set; } = new Vector3i(16, ChunkColumn.WorldHeight, 16);

        public MapChunk(Vector2i position, int sizeXZ = 16)
        {
            Position = position;

            X = position.X;
            Z = position.Z;
            ChunkSize = new Vector3i(sizeXZ, ChunkColumn.WorldHeight, sizeXZ);
            Heights = new BlockData<int>(sizeXZ);
            Biomes = new BlockData<byte>(sizeXZ);
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
            return ((z & (ChunkSize.Z - 1)) * ChunkSize.X) + (x & (ChunkSize.X - 1));
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