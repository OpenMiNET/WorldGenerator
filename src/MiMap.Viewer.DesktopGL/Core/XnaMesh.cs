using Microsoft.Xna.Framework.Graphics;

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
}