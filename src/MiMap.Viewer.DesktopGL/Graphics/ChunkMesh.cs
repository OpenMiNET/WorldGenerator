using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

            int vOffset = 0;
            int iOffset = 0;
            int vLength = 0;
            int iLength = 0;

            foreach (var mesh in meshes)
            {
                var m = mesh.ToXnaMesh_VertexPositionColorTexture();
                vLength = m.Vertices.Length;
                iLength = m.Indices.Length;
                _vertexBuffer.SetData(m.Vertices, vOffset, vLength);
                _indexBuffer.SetData(m.Indices, iOffset, iLength);
                vOffset += vLength;
                iOffset += iLength;
            }
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

        public static ChunkMesh GenerateMesh(GraphicsDevice graphicsDevice, ChunkColumn chunk)
        {
            var coords = new ChunkCoordinates(chunk.X, chunk.Z);
            var cm = new ChunkMesh(coords);

            var g = new DenseGrid3f(16, 1, 16, 0);
            var b = new Bitmap3(new Vector3i(16, ChunkColumn.WorldHeight, 16));
            var msh = new DMesh3();
            msh.EnableTriangleGroups();
            while (msh.MaxGroupID <= biomeColorMap.Length)
            {
                msh.AllocateTriangleGroup();
            }

            int z, x, y, maxY, biome, idx;
            Color color;

            for (z = 0; z < 16; z++)
            {
                for (x = 0; x < 16; x++)
                {
                    idx = ((z & 0xF) << 4) | (x & 0xF);
                    maxY = cm.Heights[idx] = chunk.height[idx];
                    biome = cm.Biomes[idx] = chunk.biomeId[idx];
                    color = cm.Colors[idx] = biomeColorMap[biome];

                    g[x, 0, z] = biome;
                    for (y = 0; y < maxY; y++)
                    {
                        b[new Vector3i(x, y, z)] = true;
                        msh.AppendCube(new Vector3i(x, y, z), biome);
                    }

                    for (; y < ChunkColumn.WorldHeight; y++)
                    {
                        b[new Vector3i(x, y, z)] = false;
                    }
                }
            }

            // var f = new MergeCoincidentEdges(msh);
            // f.MergeDistance = 0.1f;
            // f.Apply();
            // msh = f.Mesh;

            // var vx = new VoxelSurfaceGenerator();
            // vx.Voxels = b;
            // vx.ColorSourceF = v =>
            // {
            //     var c = (int)g[v.x, 0, v.z];
            //     return new Colorf((c & 0xFF) / 255f, ((c >> 8) & 0xFF) / 255f, ((c >> 16) & 0xFF) / 255f);
            // };
            // vx.Clockwise = false;
            // vx.SkipInteriorFaces = false;
            // vx.CapAtBoundary = false;
            //
            //
            // vx.Generate();
            // var m = vx.Meshes[0];
            // m.WindingNumber(Vector3d.Zero);
            //m.MergeEdges(32, 64, out var edgeInfo);

            //var op = new FaceGroupOptimizer(msh);
            //op.(16);

            //var m = op.Mesh;

            // var mergeOp = new MergeCoincidentEdges(m);
            // mergeOp.Apply();
            // m = mergeOp.Mesh;

            //m.CompactInPlace(false);

            // DMeshAABBTree3 spatial = new DMeshAABBTree3(vx.Meshes[0], autoBuild: true);
            // spatial.WindingNumber(Vector3d.Zero);
            // var mb = new MarchingCubes();
            // var sdf = new MeshSignedDistanceGrid(msh, 1f);
            // sdf.Compute();
            //
            // var iso = new DenseGridTrilinearImplicit(sdf.Grid, sdf.GridOrigin, sdf.CellSize);
            //
            // var c = new MarchingCubesPro();
            // c.RootMode = MarchingCubesPro.RootfindingModes.Bisection;
            // c.Implicit = iso;
            // c.Bounds = msh.CachedBounds;
            // c.CubeSize = c.Bounds.maxdi;
            // c.Bounds.Expand(6 * c.CubeSize);
            //
            // c.Generate();
            // DMesh3 outputMesh = c.Mesh;
            //
            // var iso = new DenseGridTrilinearImplicit(sdf.Grid, sdf.GridOrigin, sdf.CellSize);
            //
            // mb.Implicit = iso;
            // mb.Bounds = m.CachedBounds;
            // mb.CubeSize = mb.Bounds.MaxDim / 16d;
            // mb.Bounds.Expand(3 * mb.CubeSize);
            // mb.Generate();
            //msh.WindingNumber(Vector3d.Zero);

            cm.Initialize(graphicsDevice, msh);
            return cm;
        }
    }
}