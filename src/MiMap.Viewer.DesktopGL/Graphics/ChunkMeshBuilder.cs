using System.Linq;
using System.Runtime.CompilerServices;
using g3;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Core;
using MiNET.Worlds;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void StretchBoundaryFacesToZero(CubeMeshBuilder cubeMeshBuilder, Map map, ChunkMesh chunk)
        {
            int x, z;
            int idx;
            int biome;
            int height;
            Vector3i pos;
            Vector2 uv;
            Color color;
            
            for (x = 0; x < 16; x++)
            for (z = 0; z < 16; z++)
            {
                if (!(x == 0 || x == 15) && !(z == 0 || z == 15)) 
                    continue;
                
                idx = GetXZIndex(x, z);
                biome = chunk.Biomes[idx];
                height = chunk.Heights[idx];
                pos = new Vector3i(x, height, z);
                uv = biomeUvMapVector2[biome];
                color = biomeColorMap[biome];
                
                cubeMeshBuilder.AddCubeFace(pos, CubeFace.North, -height, uv, color);
            }
        }
        
        public static void GenerateMesh(Map map, ref ChunkMesh chunk)
        {
            int z, x;
            var meshBuilder = new CubeMeshBuilder();

            ReducedMesh(meshBuilder, chunk);
            
            
            // for (z = 0; z < 16; z++)
            // {
            //     for (x = 0; x < 16; x++)
            //     {
            //         CheckAndAddFaces(meshBuilder, chunk, x, z);
            //     }
            // }
            //
            // StretchBoundaryFacesToZero(meshBuilder, map, chunk);

            chunk.UpdateMesh(meshBuilder.BuildTriangulatedMesh());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int? GetField(MapChunk chunk, int x, int y, int z)
        {
            var idx = GetXZIndex(x, z);
            var h = chunk.Heights[idx];
            return y <= h ? chunk.Biomes[idx] : null;
        }

        public static void ReducedMesh(ICubeMeshBuilder cubeMeshBuilder, MapChunk chunk)
        {
            //   List<Vector2> uvs = new List<Vector2>();
            // List<Color> colors = new List<Color>();

            int[] dims = { 16, ChunkColumn.WorldHeight, 16 };

            //Sweep over 3-axes
            for (int d = 0; d < 3; d++)
            {
                int i = 0;
                int j = 0;
                int k = 0;
                int l = 0;
                int w = 0;
                int h = 0;

                int u = (d + 1) % 3;
                int v = (d + 2) % 3;

                int[] x = { 0, 0, 0 };
                int[] q = { 0, 0, 0 };
                int?[] mask = new int?[(dims[u] + 1) * (dims[v] + 1)];


                q[d] = 1;

                for (x[d] = -1; x[d] < dims[d];)
                {
                    // Compute the mask
                    int n = 0;
                    for (x[v] = 0; x[v] < dims[v]; ++x[v])
                    {
                        for (x[u] = 0; x[u] < dims[u]; ++x[u], ++n)
                        {
                            int? vox1 = GetField(chunk, x[0], x[1], x[2]);
                            int? vox2 = GetField(chunk, x[0] + q[0], x[1] + q[1], x[2] + q[2]);

//                            mask[n] = vox1 != vox2 ? 1 : 0;

                            int? a = (0 <= x[d] ? vox1 : null);
                            int? b = (x[d] < dims[d] - 1 ? vox2 : null);
                            
                            if ((vox1.HasValue) == (vox2.HasValue))
                            {
                                mask[n] = null;
                            }
                            else if ((a.HasValue))
                            {
                                mask[n] = vox1;
                            }
                            else
                            {
                                mask[n] = vox2;
                            }


                            //    bool test = (0 <= x[d] ? (int)chunk.GetField(x[0], x[1], x[2]) : 0) !=
                            //       (x[d] < dims[d] - 1 ? (int)chunk.GetField(x[0] + q[0], x[1] + q[1], x[2] + q[2]) : 0);

                            //  mask[n++] = test ? 1 : 0;
                        }
                    }

                    // Increment x[d]
                    ++x[d];

                    // Generate mesh for mask using lexicographic ordering
                    n = 0;
                    for (j = 0; j < dims[v]; ++j)
                    {
                        for (i = 0; i < dims[u];)
                        {
                            var c = mask[n];

                            if (c.HasValue)
                            {
                                // compute width
                                for (w = 1; mask[n + w] == c && (i + w) < dims[u]; ++w)
                                {
                                }

                                // compute height
                                bool done = false;
                                for (h = 1; (j + h) < dims[v]; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (mask[n + k + h * dims[u]] != c)
                                        {
                                            done = true;
                                            break;
                                        }
                                    }

                                    if (done)
                                    {
                                        break;
                                    }
                                }

                                // add quad
                                x[u] = i;
                                x[v] = j;

                                int[] du = { 0, 0, 0 };
                                int[] dv = { 0, 0, 0 };

                                if (c > 0)
                                {
                                    dv[v] = h;
                                    du[u] = w;
                                }
                                else
                                {
                                    du[v] = h;
                                    dv[u] = w;
                                }

                                Vector3 v1 = new Vector3(x[0], x[1], x[2]);
                                Vector3 v2 = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                Vector3 v3 = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]);
                                Vector3 v4 = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                                var biomeUv = biomeUvMapVector2[c.Value];
                                var biomeColor = biomeColorMap[c.Value];

                                cubeMeshBuilder.AddQuad((Vector3i)v1, (Vector3i)v2, (Vector3i)v3, (Vector3i)v4, 1, biomeUv, biomeColor);

                                for (l = 0; l < h; ++l)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        mask[n + k + l * dims[u]] = null;
                                    }
                                }

                                // increment counters
                                i += w;
                                n += w;
                            }
                            else
                            {
                                ++i;
                                ++n;
                            }
                        }
                    }
                }
            }
        }
    }
}