using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MiMap.Viewer.DesktopGL.Primitive;

namespace MiMap.Viewer.DesktopGL.Core
{
    public interface ICubeMeshBuilder
    {
        void AddCubeFace(Vector3i pos, CubeFace face, int height, Vector2 uv, Color color);
        
        IRawMesh BuildTriangulatedMesh();
        IRawMesh BuildQuadMesh();

        void OptimizeFaces();
        void AddQuad(Vector3i v1, Vector3i v2, Vector3i v3, Vector3i v4, Vector3 normal, Vector2 uv, Color color);
    }
}