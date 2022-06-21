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

    public static class DMeshExtensions
    {
        public static RawMesh<TVertex> Combine<TVertex>(this ICollection<RawMesh<TVertex>> meshes) 
            where TVertex : struct, IVertexType
        {
            int tVertices = 0, tTriangles = 0, tIndices = 0;

            foreach (var mesh in meshes)
            {
                tVertices += mesh.VertexCount;
                tTriangles += mesh.PrimitiveCount;
                tIndices += mesh.IndexCount;
            }

            var vtxs = new TVertex[tVertices];
            var inds = new int[tIndices];

            int vOffset = 0;
            int iOffset = 0;
            int vLength = 0;
            int iLength = 0;

            foreach (var mesh in meshes)
            {
                vLength = mesh.Vertices.Length;
                iLength = mesh.Indices.Length;
                Array.Copy(mesh.Vertices, 0, vtxs, vOffset, vLength);
                Array.Copy(mesh.Indices, 0, inds, iOffset, iLength);
                vOffset += vLength;
                iOffset += iLength;
            }

            return new RawMesh<TVertex>(vtxs, inds);
        }

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

        public static void AppendCube(this DMesh3 mesh, Vector3i nz, int biome, Func<Index3i, bool> checkNeighbor = null)
        {
            int[] vertices = new int[4];
            for (int fi = 0; fi < 6; ++fi)
            {
                Box3d cube = Box3d.UnitZeroCentered;
                cube.Center = (Vector3d)nz + (Vector3d.One / 2f);

                // checks dependent on neighbours
                Index3i nbr = nz + gIndices.GridOffsets6[fi];
                if (checkNeighbor?.Invoke(nbr) ?? true)
                {
                    continue;
                }

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

        public static void AppendCube2(this MeshEditor meshEditor, Vector3i nz, int biome)
        {
            var c = new Colorf((biome & 0xFF) / 255f, ((biome >> 8) & 0xFF) / 255f, ((biome >> 16) & 0xFF) / 255f);
            var uv = new Vector2f(biome, 0);

            TrivialBox3Generator trivialBox3Generator = new TrivialBox3Generator();
            trivialBox3Generator.Box = new Box3d(new Frame3f(new Vector3f(nz.x, nz.y, nz.z)), Vector3d.One);
            trivialBox3Generator.NoSharedVertices = false;
            trivialBox3Generator.Generate();
            DMesh3 dmesh3 = new DMesh3();
            trivialBox3Generator.MakeMesh(dmesh3);
            dmesh3.EnableTriangleGroups(biome);
            dmesh3.EnableVertexColors(c);
            dmesh3.EnableVertexUVs(uv);
            meshEditor.AppendMesh((IMesh)dmesh3, meshEditor.Mesh.AllocateTriangleGroup());
        }

        public static RawMesh<VertexPositionColorTexture> ToXnaMesh_VertexPositionColorTexture(this DMesh3 d3)
        {
            //d3 = new DMesh3(d3, true, MeshComponents.All);
            var vCount = d3.VertexCount;
            var vertices = new VertexPositionColorTexture[vCount];

            NewVertexInfo v = new NewVertexInfo();
            int b;
            Vector3 p;
            Vector2 uv;
            Color c;
            for (int i = 0; i < vCount; i++)
            {
                d3.GetVertex(i, ref v, bWantNormals: false, bWantColors: true, bWantUVs: true);
                p = v.v.ToXna();
                c = v.c.ToXnaColor();
                uv = v.uv.ToXna();

                vertices[i] = new VertexPositionColorTexture(p, c, uv);
            }

            var indices = d3.Triangles().SelectMany(f => new[]
            {
                f.a,
                f.b,
                f.c
            }).ToArray();

            return new RawMesh<VertexPositionColorTexture>(vertices, indices);
        }

        public static RawMesh<VertexPositionNormalTexture> ToXnaMesh_VertexPositionNormalTexture(this DMesh3 d3)
        {
            //d3 = new DMesh3(d3, true, MeshComponents.All);
            var vCount = d3.VertexCount;
            var vertices = new VertexPositionNormalTexture[vCount];

            NewVertexInfo v = new NewVertexInfo();
            int b;
            Vector3 p;
            Vector3 n;
            Vector2 uv;

            for (int i = 0; i < vCount; i++)
            {
                d3.GetVertex(i, ref v, bWantNormals: true, bWantColors: false, bWantUVs: true);
                p = v.v.ToXna();
                n = v.n.ToXna();
                uv = v.uv.ToXna();

                vertices[i] = new VertexPositionNormalTexture(p, n, uv);
            }

            var indices = d3.Triangles().SelectMany(f => new[]
            {
                f.a,
                f.b,
                f.c
            }).ToArray();

            return new RawMesh<VertexPositionNormalTexture>(vertices, indices);
        }
    }

    public static class D3VectorExtensions
    {
        public static Vector3 ToXna(this Vector3d v) => new Vector3((float)v.x, (float)v.y, (float)v.z);
        public static Vector3 ToXna(this Vector3f v) => new Vector3(v.x, v.y, v.z);
        public static Vector2 ToXna(this Vector2f v) => new Vector2(v.x, v.y);
        public static Vector3 ToXna(this System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
        public static System.Numerics.Vector3 ToNumerics(this Vector3 v) => new(v.X, v.Y, v.Z);

        public static Vector3f ToVector3f(this Color v) => new Vector3f(v.R / 255f, v.G / 255f, v.B / 255f);
        public static Vector3f ToVector3f(this System.Drawing.Color v) => new Vector3f(v.R / 255f, v.G / 255f, v.B / 255f);
        public static Color ToXnaColor(this Vector3f v) => new Color(v.x, v.y, v.z);
        public static Color ToXnaColor(this System.Drawing.Color v) => new Color(v.R, v.G, v.B, v.A);
    }
}