using System.Linq;
using System.Runtime.CompilerServices;
using g3;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Core;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public static class ChunkMeshBuilder
    {
        public static Color[] biomeColorMap;
        public static Vector3f[] biomeColorMapVector3f;
        public static Vector2f[] biomeUvMapVector2f;

        public static void InitializeColorMap(BiomeRegistry registry)
        {
            var biomes = registry.GetBiomes();
            var maxBiome = biomes.Max(b => b.Id);
            biomeColorMap = new Color[maxBiome + 1];
            biomeColorMapVector3f = new Vector3f[maxBiome + 1];
            biomeUvMapVector2f = new Vector2f[maxBiome + 1];
            for (int i = 0; i <= maxBiome; i++)
            {
                biomeColorMap[i] = Color.Magenta;
                biomeColorMapVector3f[i] = Color.Magenta.ToVector3f();
                biomeUvMapVector2f[i] = new Vector2f(i, 0);
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
        public static void CheckAndAddFaces(DMesh3 mesh, MapChunk chunk, int x, int z)
        {
            int tIdx, idx;
            int tBiome, biome;
            int tHeight, height;
            int[] vertices = new int[4];
            NewVertexInfo vi = new NewVertexInfo();

            idx = GetXZIndex(x, z);
            biome = chunk.Biomes[idx];
            height = chunk.Heights[idx];

            vi.bHaveC = true;
            vi.c = biomeColorMapVector3f[biome];
            // vi.c = new Colorf((biome & 0xFF) / 255f, ((biome >> 8) & 0xFF) / 255f, ((biome >> 16) & 0xFF) / 255f);
            
            vi.bHaveUV = true;
            vi.uv = biomeUvMapVector2f[biome];


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
                    vi.bHaveN = true;
                    vi.n = -Vector3f.AxisX;

                    // @formatter:off
                    vi.v = new Vector3d(x    , tHeight, z    ); vertices[0] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x    ,  height, z    ); vertices[1] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x    ,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x    , tHeight, z + 1); vertices[3] = mesh.AppendVertex(vi);

                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[1], vertices[2]), biome);
                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[3]), biome);
                    // @formatter:on
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
                    vi.bHaveN = true;
                    vi.n = Vector3f.AxisX;

                    // @formatter:off
                    vi.v = new Vector3d(x + 1, tHeight, z    ); vertices[0] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1,  height, z    ); vertices[1] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1, tHeight, z + 1); vertices[3] = mesh.AppendVertex(vi);

                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[1]), biome);
                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[3], vertices[2]), biome);
                    // @formatter:on
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
                    vi.bHaveN = true;
                    vi.n = -Vector3f.AxisZ;

                    // @formatter:off
                    vi.v = new Vector3d(x    , tHeight, z    ); vertices[0] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x    ,  height, z    ); vertices[1] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1,  height, z    ); vertices[2] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1, tHeight, z    ); vertices[3] = mesh.AppendVertex(vi);

                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[1], vertices[2]), biome);
                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[3]), biome);
                    // @formatter:on
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
                    vi.bHaveN = true;
                    vi.n = Vector3f.AxisZ;

                    // @formatter:off
                    vi.v = new Vector3d(x    , tHeight, z + 1); vertices[0] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x    ,  height, z + 1); vertices[1] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                    vi.v = new Vector3d(x + 1, tHeight, z + 1); vertices[3] = mesh.AppendVertex(vi);

                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[1]), biome);
                    mesh.AppendTriangle(new Index3i(vertices[0], vertices[3], vertices[2]), biome);
                    // @formatter:on
                }
            }

            // +Y
            {
                vi.bHaveN = true;
                vi.n = Vector3f.AxisY;

                // @formatter:off
                vi.v = new Vector3d(x    ,  height, z    ); vertices[0] = mesh.AppendVertex(vi);
                vi.v = new Vector3d(x    ,  height, z + 1); vertices[1] = mesh.AppendVertex(vi);
                vi.v = new Vector3d(x + 1,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                vi.v = new Vector3d(x + 1,  height, z    ); vertices[3] = mesh.AppendVertex(vi);

                mesh.AppendTriangle(new Index3i(vertices[0], vertices[1], vertices[2]), biome);
                mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[3]), biome);
                // @formatter:on
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void CheckAndAddBoundaryFaces(DMesh3 mesh, Map map, ChunkMesh chunk)
        {
            ChunkMesh tChunk;
            int x, z, tX, tZ;
            int tIdx, idx;
            int tBiome, biome;
            int tHeight, height;
            int[] vertices = new int[4];
            NewVertexInfo vi = new NewVertexInfo();
            
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

                    vi.bHaveC = true;
                    vi.c = biomeColorMapVector3f[biome];
                    // vi.c = new Colorf((biome & 0xFF) / 255f, ((biome >> 8) & 0xFF) / 255f, ((biome >> 16) & 0xFF) / 255f);
            
                    vi.bHaveUV = true;
                    vi.uv = biomeUvMapVector2f[biome];


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
                        vi.bHaveN = true;
                        vi.n = -Vector3f.AxisX;

                        // @formatter:off
                        vi.v = new Vector3d(x    , tHeight, z    ); vertices[0] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x    ,  height, z    ); vertices[1] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x    ,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x    , tHeight, z + 1); vertices[3] = mesh.AppendVertex(vi);

                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[1], vertices[2]), biome);
                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[3]), biome);
                        // @formatter:on
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
                        vi.bHaveN = true;
                        vi.n = Vector3f.AxisX;

                        // @formatter:off
                        vi.v = new Vector3d(x + 1, tHeight, z    ); vertices[0] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1,  height, z    ); vertices[1] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1, tHeight, z + 1); vertices[3] = mesh.AppendVertex(vi);

                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[1]), biome);
                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[3], vertices[2]), biome);
                        // @formatter:on
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
                        vi.bHaveN = true;
                        vi.n = -Vector3f.AxisZ;

                        // @formatter:off
                        vi.v = new Vector3d(x    , tHeight, z    ); vertices[0] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x    ,  height, z    ); vertices[1] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1,  height, z    ); vertices[2] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1, tHeight, z    ); vertices[3] = mesh.AppendVertex(vi);

                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[1], vertices[2]), biome);
                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[3]), biome);
                        // @formatter:on
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
                        vi.bHaveN = true;
                        vi.n = Vector3f.AxisZ;

                        // @formatter:off
                        vi.v = new Vector3d(x    , tHeight, z + 1); vertices[0] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x    ,  height, z + 1); vertices[1] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1,  height, z + 1); vertices[2] = mesh.AppendVertex(vi);
                        vi.v = new Vector3d(x + 1, tHeight, z + 1); vertices[3] = mesh.AppendVertex(vi);

                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[2], vertices[1]), biome);
                        mesh.AppendTriangle(new Index3i(vertices[0], vertices[3], vertices[2]), biome);
                        // @formatter:on
                    }
                }
            }
            
            chunk.HasAllMeshNeighbors = (neighborCount == 4);
        }

        public static void GenerateMesh(Map map, ref ChunkMesh chunk)
        {
            var msh = new DMesh3(MeshComponents.All);

            msh.EnableTriangleGroups();
            while (msh.MaxGroupID <= biomeColorMap.Length)
            {
                msh.AllocateTriangleGroup();
            }

            int z, x;

            for (z = 0; z < 16; z++)
            {
                for (x = 0; x < 16; x++)
                {
                    CheckAndAddFaces(msh, chunk, x, z);
                }
            }

            CheckAndAddBoundaryFaces(msh, map, chunk);

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

            chunk.UpdateMesh(msh.ToXnaMesh_VertexPositionColorTexture());
        }
    }
}