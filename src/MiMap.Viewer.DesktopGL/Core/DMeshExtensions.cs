using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Graphics;
using OpenAPI.WorldGenerator.Generators.Biomes;

namespace MiMap.Viewer.DesktopGL.Core
{
    public class XnaMesh
    {
        public GraphicsDevice GraphicsDevice { get; }
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int NumberOfVertices { get; set; }
        public int NumberOfPrimitives { get; set; }

        public Effect Effect { get; set; }

        public void Draw()
        {
        }
    }

    public class RawMesh<T>
    {
        public T[] Vertices;
        public int[] Indices;
        public int TriangleCount;

        public RawMesh(T[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
            TriangleCount = indices.Length / 3;
        }
    }

    public static class DMeshExtensions
    {
        public static RawMesh<VertexPositionColor> ToXnaMesh_VertexPositionColor(this DMesh3 d3)
        {
            d3 = new DMesh3(d3, true, MeshComponents.All);
            var vCount = d3.VertexCount;
            var vertices = new VertexPositionColor[vCount];

            NewVertexInfo v = new NewVertexInfo();
            for (int i = 0; i < vCount; i++)
            {
                d3.GetVertex(i, ref v, bWantNormals: false, bWantColors: true, bWantUVs: false);
                vertices[i] = new VertexPositionColor(v.v.ToXna(), v.c.ToXnaColor());
            }

            var indices = d3.Triangles().SelectMany(f => new[]
            {
                f.a,
                f.b,
                f.c
            }).ToArray();

            return new RawMesh<VertexPositionColor>(vertices, indices);
        }

        public static void AppendCube(this DMesh3 mesh, Vector3i nz, int biome)
        {
            int[] vertices = new int[4];
            for (int fi = 0; fi < 6; ++fi)
            {
                Box3d cube = Box3d.UnitZeroCentered;
                cube.Center = (Vector3d)nz + (Vector3d.One / 2f);

                // checks dependent on neighbours
                Index3i nbr = nz + gIndices.GridOffsets6[fi];

                int ni = gIndices.BoxFaceNormals[fi];
                Vector3f n = (Vector3f)(Math.Sign(ni) * cube.Axis(Math.Abs(ni) - 1));
                NewVertexInfo vi = new NewVertexInfo(Vector3d.Zero, n);
                vi.c = new Colorf((biome & 0xFF) / 255f, ((biome >> 8) & 0xFF) / 255f, ((biome >> 16) & 0xFF) / 255f);
                vi.bHaveC = true;
                vi.uv = new Vector2f(biome, 0);
                vi.bHaveUV = true;


                for (int j = 0; j < 4; ++j)
                {
                    vi.v = cube.Corner(gIndices.BoxFaces[fi, j]);
                    vertices[j] = mesh.AppendVertex(vi);
                }

                Index3i t0 = new Index3i(vertices[0], vertices[1], vertices[2], false);
                Index3i t1 = new Index3i(vertices[0], vertices[2], vertices[3], false);
                mesh.AppendTriangle(t0, biome);
                mesh.AppendTriangle(t1, biome);
            }
        }

        public static RawMesh<VertexPositionColorTexture> ToXnaMesh_VertexPositionColorTexture(this DMesh3 d3)
        {
            d3 = new DMesh3(d3, true, MeshComponents.All);
            var vCount = d3.VertexCount;
            var vertices = new VertexPositionColorTexture[vCount];

            NewVertexInfo v = new NewVertexInfo();
            int b;
            Color c;
            for (int i = 0; i < vCount; i++)
            {
                d3.GetVertex(i, ref v, bWantNormals: false, bWantColors: true, bWantUVs: true);
                b = (int)(v.c.x * 255f) | (((int)(v.c.y * 255f) << 8) & 0xFF00) | (((int)(v.c.z * 255f) << 16) & 0xFF0000);
                // b = (int)v.uv.x;
                c = ChunkMeshBuilder.biomeColorMap[b];

                vertices[i] = new VertexPositionColorTexture(v.v.ToXna(), c, new Vector2(b, 0));
            }

            var indices = d3.Triangles().SelectMany(f => new[]
            {
                f.a,
                f.b,
                f.c
            }).ToArray();

            return new RawMesh<VertexPositionColorTexture>(vertices, indices);
        }
    }

    public static class D3VectorExtensions
    {
        public static Vector3 ToXna(this Vector3d v) => new Vector3((float)v.x, (float)v.y, (float)v.z);
        public static Vector3 ToXna(this Vector3f v) => new Vector3(v.x, v.y, v.z);
        public static Vector3 ToXna(this System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
        public static System.Numerics.Vector3 ToNumerics(this Vector3 v) => new(v.X, v.Y, v.Z);
        public static Color ToXnaColor(this Vector3f v) => new Color(v.x, v.y, v.z);
        public static Color ToXnaColor(this System.Drawing.Color v) => new Color(v.R, v.G, v.B, v.A);
    }
}