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
    }
}