using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Graphics.Effects;
using MiMap.Viewer.DesktopGL.Primitive;
using MiNET;

namespace MiMap.Viewer.DesktopGL.Core
{
    public enum CubeFace : int
    {
        Down = 0,
        Up = 1,
        North = 2,
        South = 3,
        West = 4,
        East = 5
    }

    public class CubeMeshBuilder : ICubeMeshBuilder
    {
        private static readonly Vector3[] FaceNormal = new[]
        {
            new Vector3(0, -1, 0), // -Y
            new Vector3(0, 1, 0), // +Y
            new Vector3(0, 0, 1), // +Z
            new Vector3(0, 0, -1), // -Z
            new Vector3(-1, 0, 0), // -X
            new Vector3(1, 0, 0), // +X
        };

        private static readonly Vector3i[,] FaceVertexOffsets = new[,]
        {
            { new Vector3i(0, 0, 0), new Vector3i(1, 0, 0), new Vector3i(1, 0, 1), new Vector3i(0, 0, 1) }, // -Y
            { new Vector3i(0, 1, 1), new Vector3i(1, 1, 1), new Vector3i(1, 1, 0), new Vector3i(0, 1, 0) }, // +Y
            { new Vector3i(1, 1, 1), new Vector3i(0, 1, 1), new Vector3i(0, 0, 1), new Vector3i(1, 0, 1) }, // +Z
            { new Vector3i(0, 1, 0), new Vector3i(1, 1, 0), new Vector3i(1, 0, 0), new Vector3i(0, 0, 0) }, // -Z
            { new Vector3i(0, 1, 1), new Vector3i(0, 1, 0), new Vector3i(0, 0, 0), new Vector3i(0, 0, 1) }, // -X
            { new Vector3i(1, 1, 0), new Vector3i(1, 1, 1), new Vector3i(1, 0, 1), new Vector3i(1, 0, 0) }, // +X
        };

        public CubeMeshBuilder()
        {
            _vertices = new List<VertexPositionNormalTexture>();
            _indices = new List<IndexQuad>();
        }

        private List<VertexPositionNormalTexture> _vertices;
        private int _iVertices = -1;
        private List<IndexQuad> _indices;

        public void AddCubeFace(Vector3i pos, CubeFace face, int height, Vector2 uv, Color color)
        {
            var faceOffsetsIndex = (int)face;
            var indices = new IndexQuad();
            if (height < 0)
            {
                pos.Y += height;
                height = Math.Abs(height) + 1;
            }

            if (height == 0)
                height = 1;
            
            var size = new Vector3i(1, height, 1);

            for (int i = 0; i < 4; i++)
            {
                indices[i] = AppendVertex(new VertexPositionNormalTexture(pos + (FaceVertexOffsets[faceOffsetsIndex, i] * size), 
                    FaceNormal[faceOffsetsIndex],
                    uv
                ));
            }

            _indices.Add(indices);
        }

        private int AppendVertex(VertexPositionNormalTexture vertex)
        {
            _vertices.Add(vertex);
            _iVertices++;
            return _iVertices;
        }

        public IRawMesh BuildQuadMesh()
        {
            var vtx = _vertices.ToArray();
            var idx = _indices.SelectMany(i => i.ToArray()).ToArray();
            
            var mesh = new RawMesh<VertexPositionNormalTexture>(vtx, idx, PrimitiveType.PatchListWith4ControlPoints);
            _vertices.Clear();
            _vertices = null;
            _indices.Clear();
            _indices = null;
        
            return mesh;
        }
        
        public IRawMesh BuildTriangulatedMesh()
        {
            // var vtx = _vertices.Select(v => new VertexPositionNormalTexture(v.Position, v.Normal, v.TextureCoordinate)).ToArray();
            var vtx = _vertices.ToArray();
            // var idx = _indices.SelectMany(i => new[]
            // {
            //     i[0], 
            //     i[1], 
            //     i[3], 
            //     i[3], 
            //     i[1], 
            //     i[2]
            // }).ToArray();
            var idx = _indices.SelectMany(i => new[]
            {
                i[3], 
                i[0], 
                i[2], 
                i[2], 
                i[0], 
                i[1]
            }).ToArray();

            var mesh = new RawMesh<VertexPositionNormalTexture>(vtx, idx, PrimitiveType.TriangleList);
            _vertices.Clear();
            _vertices = null;
            _indices.Clear();
            _indices = null;

            return mesh;
        }
    }
}