using System.Linq;
using System.Runtime.CompilerServices;
using g3;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Core;
using OpenAPI.WorldGenerator.Generators.Biomes;
using Vector3i = MiMap.Viewer.DesktopGL.Primitive.Vector3i;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public static class ChunkMeshBuilder
    {
        public static Color[] biomeColorMap;
        public static Vector3f[] biomeColorMapVector3f;
        public static Vector2[] biomeUvMapVector2;

        public static void InitializeColorMap(BiomeRegistry registry)
        {
            var biomes = registry.GetBiomes();
            var maxBiome = biomes.Max(b => b.Id);
            biomeColorMap = new Color[maxBiome + 1];
            biomeColorMapVector3f = new Vector3f[maxBiome + 1];
            biomeUvMapVector2 = new Vector2[maxBiome + 1];
            for (int i = 0; i <= maxBiome; i++)
            {
                biomeColorMap[i] = Color.Magenta;
                biomeColorMapVector3f[i] = Color.Magenta.ToVector3f();
                biomeUvMapVector2[i] = new Vector2(i / (float)maxBiome, 0);
                //biomeUvMapVector2[i] = new Vector2(i, 0);
            }

            foreach (var biome in biomes)
            {
                var color = biome.Color.GetValueOrDefault(System.Drawing.Color.DarkMagenta);
                biomeColorMap[biome.Id] = new Color(color.R, color.G, color.B, color.A);
                biomeColorMapVector3f[biome.Id] = new Vector3f(color.R, color.G, color.B);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int GetXZIndex(int x, int z)
        {
            return ((z & 0xF) << 4) | (x & 0xF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void CheckAndAddFaces(ICubeMeshBuilder cubeMeshBuilder, MapChunk chunk, int x, int z)
        {
            int tIdx, idx;
            int tBiome, biome;
            int tHeight, height;
            Vector3i pos;
            Vector2 uv;
            Color color;


            idx = GetXZIndex(x, z);
            biome = chunk.Biomes[idx];
            height = chunk.Heights[idx];
            
            pos = new Vector3i(x, height, z);
            uv = biomeUvMapVector2[biome];
            color = biomeColorMap[biome];

            // -X
            if (x > 0)
            {
                tIdx = GetXZIndex(x - 1, z);
                tBiome = chunk.Biomes[tIdx];
                tHeight = chunk.Heights[tIdx];
                if (biome != tBiome)
                {
                    // tHeight = 0;
                }

                // add side
                if (height > tHeight)
                {
                    cubeMeshBuilder.AddCubeFace(pos, CubeFace.West, tHeight - height, uv, color);
                }
            }

            // +X
            if (x < 15)
            {
                tIdx = GetXZIndex(x + 1, z);
                tBiome = chunk.Biomes[tIdx];
                tHeight = chunk.Heights[tIdx];
                if (biome != tBiome)
                {
                    //tHeight = 0;
                }

                // add side
                if (height > tHeight)
                {
                    cubeMeshBuilder.AddCubeFace(pos, CubeFace.East, tHeight - height, uv, color);
                }
            }

            // -Z
            if (z > 0)
            {
                tIdx = GetXZIndex(x, z - 1);
                tBiome = chunk.Biomes[tIdx];
                tHeight = chunk.Heights[tIdx];
                if (biome != tBiome)
                {
                    // tHeight = 0;
                }

                // add side
                if (height > tHeight)
                {
                    cubeMeshBuilder.AddCubeFace(pos, CubeFace.South, tHeight - height, uv, color);
                }
            }

            // +Z
            if (z < 15)
            {
                tIdx = GetXZIndex(x, z + 1);
                tBiome = chunk.Biomes[tIdx];
                tHeight = chunk.Heights[tIdx];
                if (biome != tBiome)
                {
                    //tHeight = 0;
                }

                // add side
                if (height > tHeight)
                {
                    cubeMeshBuilder.AddCubeFace(pos, CubeFace.North, tHeight - height, uv, color);
                }
            }

            // +Y
            {
                cubeMeshBuilder.AddCubeFace(pos, CubeFace.Up, 1, uv, color);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void CheckAndAddBoundaryFaces(ICubeMeshBuilder cubeMeshBuilder, Map map, ChunkMesh chunk)
        {
            ChunkMesh tChunk;
            int x, z, tX, tZ;
            int tIdx, idx;
            int tBiome, biome;
            int tHeight, height;
            Vector3i pos;
            Vector2 uv;
            Color color;
            
            int neighborCount = 0;

            // -X
            if (map.TryGetChunk(chunk.X - 1, chunk.Z, out tChunk))
            {
                neighborCount++;
                
                x = 0;
                tX = 15;

                for (z = 0, tZ = 0; z < 16; z++, tZ++)
                {
                    idx = GetXZIndex(x, z);
                    biome = chunk.Biomes[idx];
                    height = chunk.Heights[idx];

                    pos = new Vector3i(x, height, z);
                    uv = biomeUvMapVector2[biome];
                    color = biomeColorMap[biome];


                    tIdx = GetXZIndex(tX, tZ);
                    tBiome = tChunk.Biomes[tIdx];
                    tHeight = tChunk.Heights[tIdx];
                    if (biome != tBiome)
                    {
                        //tHeight = 0;
                    }

                    // add side
                    if (height > tHeight)
                    {
                        cubeMeshBuilder.AddCubeFace(pos, CubeFace.West, tHeight-height, uv, color);
                    }
                }
            }

            // +X
            if (map.TryGetChunk(chunk.X + 1, chunk.Z, out tChunk))
            {
                neighborCount++;
                
                x = 15;
                tX = 0;

                for (z = 0, tZ = 0; z < 16; z++, tZ++)
                {
                    idx = GetXZIndex(x, z);
                    biome = chunk.Biomes[idx];
                    height = chunk.Heights[idx];
                    pos = new Vector3i(x, height, z);
                    uv = biomeUvMapVector2[biome];
                    color = biomeColorMap[biome];

                    tIdx = GetXZIndex(tX, tZ);
                    tBiome = tChunk.Biomes[tIdx];
                    tHeight = tChunk.Heights[tIdx];
                    if (biome != tBiome)
                    {
                        //tHeight = 0;
                    }

                    // add side
                    if (height > tHeight)
                    {
                        cubeMeshBuilder.AddCubeFace(pos, CubeFace.East, tHeight-height, uv, color);
                    }
                }
            }

            // -Z
            if (map.TryGetChunk(chunk.X, chunk.Z - 1, out tChunk))
            {
                neighborCount++;

                z = 0;
                tZ = 15;

                for (x = 0, tX = 0; x < 16; x++, tX++)
                {
                    idx = GetXZIndex(x, z);
                    biome = chunk.Biomes[idx];
                    height = chunk.Heights[idx];
                    pos = new Vector3i(x, height, z);
                    uv = biomeUvMapVector2[biome];
                    color = biomeColorMap[biome];

                    tIdx = GetXZIndex(tX, tZ);
                    tBiome = tChunk.Biomes[tIdx];
                    tHeight = tChunk.Heights[tIdx];
                    if (biome != tBiome)
                    {
                        //tHeight = 0;
                    }

                    // add side
                    if (height > tHeight)
                    {
                        cubeMeshBuilder.AddCubeFace(pos, CubeFace.South, tHeight-height, uv, color);
                    }
                }
            }

            // +Z
            if (map.TryGetChunk(chunk.X, chunk.Z + 1, out tChunk))
            {
                neighborCount++;

                z = 15;
                tZ = 0;

                for (x = 0, tX = 0; x < 16; x++, tX++)
                {
                    idx = GetXZIndex(x, z);
                    biome = chunk.Biomes[idx];
                    height = chunk.Heights[idx];
                    pos = new Vector3i(x, height, z);
                    uv = biomeUvMapVector2[biome];
                    color = biomeColorMap[biome];

                    tIdx = GetXZIndex(tX, tZ);
                    tBiome = tChunk.Biomes[tIdx];
                    tHeight = tChunk.Heights[tIdx];
                    if (biome != tBiome)
                    {
                        //tHeight = 0;
                    }

                    // add side
                    if (height > tHeight)
                    {
                        cubeMeshBuilder.AddCubeFace(pos, CubeFace.North, tHeight-height, uv, color);
                    }
                }
            }
            
            chunk.HasAllMeshNeighbors = (neighborCount == 4);
        }

        public static void GenerateMesh(Map map, ref ChunkMesh chunk)
        {
            

            int z, x;
            var meshBuilder = new CubeMeshBuilder();

            for (z = 0; z < 16; z++)
            {
                for (x = 0; x < 16; x++)
                {
                    CheckAndAddFaces(meshBuilder, chunk, x, z);
                }
            }

            CheckAndAddBoundaryFaces(meshBuilder, map, chunk);

            // var resultMesh = new DMesh3(MeshComponents.VertexColors | MeshComponents.VertexUVs);
            // resultMesh.CompactCopy(msh, false, true, true);

            //var reMsh = new DMesh3(msh, true, MeshComponents.All);
            //var compactInfo = reMsh.CompactCopy(msh, true, true, true);
            //msh = null;


            // var r = new Remesher(reMsh);
            // r.EnableCollapses = true;
            // r.EnableSmoothing = true;
            // r.EnableSplits = false;
            // r.EnableFlips = false;
            //
            // r.SetTargetEdgeLength(1.0f);
            // r.Precompute();
            //
            //
            // for (int i = 0; i < 1; i++)
            // {
            //     r.BasicRemeshPass();
            // }

            chunk.UpdateMesh(meshBuilder.BuildTriangulatedMesh());
        }
    }
}