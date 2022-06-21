using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Core
{
    public interface IRawMesh
    {
        int[] Indices { get; }
        PrimitiveType PrimitiveType { get; }
        int PrimitiveCount { get; }
        int VertexCount { get; }
        int IndexCount { get; }

        void CreateBuffers(GraphicsDevice graphicsDevice, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer, out int primitiveCount, out PrimitiveType primitiveType);
    }
}