using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Primitive
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VoxelVertex : IVertexType
    {
        public Vector3 PositionF;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        
        public Vector3i Position
        {
            get => (Vector3i)PositionF;
            set => PositionF = value;
        }
        
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

        public VoxelVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            PositionF = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration => VoxelVertex.VertexDeclaration;

        public override int GetHashCode() => (Position.GetHashCode() * 397 ^ Normal.GetHashCode()) * 397 ^ TextureCoordinate.GetHashCode();

        public override string ToString() => "{{Position:" + Position + " Normal:" + Normal + " TextureCoordinate:" + TextureCoordinate + "}}";

        public static bool operator ==(
            VoxelVertex left,
            VoxelVertex right)
        {
            return left.Position == right.Position && left.Normal == right.Normal && left.TextureCoordinate == right.TextureCoordinate;
        }

        public static bool operator !=(
            VoxelVertex left,
            VoxelVertex right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj) => obj != null && !(obj.GetType() != GetType()) && this == (VoxelVertex) obj;
    }
}