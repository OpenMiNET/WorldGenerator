using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using g3;
using gs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Core;
using MiNET.Utils.Vectors;
using MiNET.Worlds;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public class ChunkMesh : MapChunk, IDisposable
    {
        private GraphicsDevice _graphicsDevice;
        public ChunkCoordinates ChunkCoordinates { get; }
        public RawMesh<VertexPositionColor> Mesh { get; private set; }
        public Matrix World { get; private set; }

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private int _primitiveCount;

        public ChunkMesh(ChunkCoordinates chunkCoordinates) : base(chunkCoordinates.X, chunkCoordinates.Z)
        {
            ChunkCoordinates = chunkCoordinates;
            World = Matrix.Identity
                    * Matrix.CreateTranslation(new Vector3((chunkCoordinates.X << 4), 0f, (chunkCoordinates.Z << 4)));
            // World = Matrix.CreateTranslation(new Vector3((chunkCoordinates.X << 4), 0f, (chunkCoordinates.Z << 4)));
        }


        internal void Initialize(GraphicsDevice graphicsDevice, DMesh3 mesh)
        {
            _graphicsDevice = graphicsDevice;
            var tVertices = mesh.VertexCount;
            var tTriangles = mesh.TriangleCount;
            var tIndices = tTriangles * 3;
            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColorTexture.VertexDeclaration, tVertices, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, tIndices, BufferUsage.WriteOnly);
            _primitiveCount = tTriangles;

            int vOffset = 0;
            int iOffset = 0;
            int vLength = 0;
            int iLength = 0;

            var m = mesh.ToXnaMesh_VertexPositionColorTexture();
            vLength = m.Vertices.Length;
            iLength = m.Indices.Length;
            _vertexBuffer.SetData(m.Vertices, vOffset, vLength);
            _indexBuffer.SetData(m.Indices, iOffset, iLength);
            vOffset += vLength;
            iOffset += iLength;
        }

        internal void Initialize(GraphicsDevice graphicsDevice, ICollection<DMesh3> meshes)
        {
            _graphicsDevice = graphicsDevice;
            var tVertices = meshes.Sum(m => m.VertexCount);
            var tTriangles = meshes.Sum(m => m.TriangleCount);
            var tIndices = tTriangles * 3;
            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionColorTexture.VertexDeclaration, tVertices, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, tIndices, BufferUsage.WriteOnly);
            _primitiveCount = tTriangles;
            var vtxs = new VertexPositionColorTexture[tVertices];
            var inds = new int[tIndices];

            int vOffset = 0;
            int iOffset = 0;
            int vLength = 0;
            int iLength = 0;

            foreach (var mesh in meshes)
            {
                var m = mesh.ToXnaMesh_VertexPositionColorTexture();
                vLength = m.Vertices.Length;
                iLength = m.Indices.Length;
                Array.Copy(m.Vertices, 0, vtxs, vOffset, vLength);
                Array.Copy(m.Indices, 0, inds, iOffset, iLength);
                vOffset += vLength;
                iOffset += iLength;
            }

            _vertexBuffer.SetData(vtxs);
            _indexBuffer.SetData(inds);
        }

        public void Draw()
        {
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }

    public class ChunkMeshBuilder
    {
        public static Color[] biomeColorMap;

        public static void InitializeColorMap(BiomeRegistry registry)
        {
            var biomes = registry.GetBiomes();
            var maxBiome = biomes.Max(b => b.Id);
            biomeColorMap = new Color[maxBiome + 1];
            for (int i = 0; i <= maxBiome; i++)
                biomeColorMap[i] = Color.Magenta;

            foreach (var biome in biomes)
            {
                var color = biome.Color.GetValueOrDefault(System.Drawing.Color.DarkMagenta);
                biomeColorMap[biome.Id] = new Color(color.R, color.G, color.B, color.A);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int GetXZIndex(int x, int z)
        {
            return ((z & 0xF) << 4) | (x & 0xF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void CheckAndAddFaces(DMesh3 mesh, ChunkColumn chunk, int x, int z)
        {
            int tIdx, idx;
            int tBiome, biome;
            int tHeight, height;
            int[] vertices = new int[4];
            NewVertexInfo vi = new NewVertexInfo();

            idx = GetXZIndex(x, z);
            biome = chunk.biomeId[idx];
            height = chunk.height[idx];

            vi.bHaveC = true;
            vi.bHaveUV = true;
            vi.c = new Colorf((biome & 0xFF) / 255f, ((biome >> 8) & 0xFF) / 255f, ((biome >> 16) & 0xFF) / 255f);
            vi.uv = new Vector2f(biome, 0);


            // -X
            if (x > 0)
            {
                tIdx = GetXZIndex(x - 1, z);
                tBiome = chunk.biomeId[tIdx];
                tHeight = chunk.height[tIdx];
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
                tBiome = chunk.biomeId[tIdx];
                tHeight = chunk.height[tIdx];
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
                tBiome = chunk.biomeId[tIdx];
                tHeight = chunk.height[tIdx];
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
                tBiome = chunk.biomeId[tIdx];
                tHeight = chunk.height[tIdx];
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

        public static ChunkMesh GenerateMesh(GraphicsDevice graphicsDevice, ChunkColumn chunk)
        {
            var coords = new ChunkCoordinates(chunk.X, chunk.Z);
            var cm = new ChunkMesh(coords);

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

            // var resultMesh = new DMesh3(MeshComponents.VertexColors | MeshComponents.VertexUVs);
            // resultMesh.CompactCopy(msh, false, true, true);

            cm.Initialize(graphicsDevice, msh);

            return cm;
        }
    }
}