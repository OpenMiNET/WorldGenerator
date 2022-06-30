using System;
using Microsoft.Xna.Framework.Graphics;

namespace MiMap.Viewer.DesktopGL.Core
{
    public class RawMesh<TVertex> : IRawMesh
        where TVertex : struct, IVertexType
    {
        public TVertex[] Vertices { get; }
        public int[] Indices { get; }
        public PrimitiveType PrimitiveType { get; }
        public int PrimitiveCount { get; }
        public int VertexCount { get; }
        public int IndexCount { get; }

        public RawMesh(TVertex[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            Vertices = vertices;
            Indices = indices;
            VertexCount = vertices.Length;
            IndexCount = indices.Length;
            PrimitiveType = primitiveType;
            PrimitiveCount = indices.Length / GetPrimitiveSize(primitiveType);
        }

        public void CreateBuffers(GraphicsDevice graphicsDevice, out VertexBuffer vertexBuffer, out IndexBuffer indexBuffer, out int primitiveCount, out PrimitiveType primitiveType)
        {
            vertexBuffer = new VertexBuffer(graphicsDevice, default(TVertex).VertexDeclaration, VertexCount, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, IndexCount, BufferUsage.WriteOnly);
            primitiveCount = PrimitiveCount;
            primitiveType = PrimitiveType;

            vertexBuffer.SetData(Vertices, 0, VertexCount);
            indexBuffer.SetData(Indices, 0, IndexCount);
        }

        private int GetPrimitiveSize(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return 2;
                case PrimitiveType.TriangleList:
                    return 3;
                
                case PrimitiveType.PatchListWith1ControlPoints:
                    return 1;

                case PrimitiveType.PatchListWith2ControlPoints:
                    return 2;

                case PrimitiveType.PatchListWith3ControlPoints:
                    return 3;

                case PrimitiveType.PatchListWith4ControlPoints:
                    return 4;

                case PrimitiveType.PatchListWith5ControlPoints:
                    return 5;

                case PrimitiveType.PatchListWith6ControlPoints:
                    return 6;

                case PrimitiveType.PatchListWith7ControlPoints:
                    return 7;

                case PrimitiveType.PatchListWith8ControlPoints:
                    return 8;

                case PrimitiveType.PatchListWith9ControlPoints:
                    return 9;

                case PrimitiveType.PatchListWith10ControlPoints:
                    return 10;

                case PrimitiveType.PatchListWith11ControlPoints:
                    return 11;

                case PrimitiveType.PatchListWith12ControlPoints:
                    return 12;

                case PrimitiveType.PatchListWith13ControlPoints:
                    return 13;

                case PrimitiveType.PatchListWith14ControlPoints:
                    return 14;

                case PrimitiveType.PatchListWith15ControlPoints:
                    return 15;

                case PrimitiveType.PatchListWith16ControlPoints:
                    return 16;

                case PrimitiveType.PatchListWith17ControlPoints:
                    return 17;

                case PrimitiveType.PatchListWith18ControlPoints:
                    return 18;

                case PrimitiveType.PatchListWith19ControlPoints:
                    return 19;

                case PrimitiveType.PatchListWith20ControlPoints:
                    return 20;

                case PrimitiveType.PatchListWith21ControlPoints:
                    return 21;

                case PrimitiveType.PatchListWith22ControlPoints:
                    return 22;

                case PrimitiveType.PatchListWith23ControlPoints:
                    return 23;

                case PrimitiveType.PatchListWith24ControlPoints:
                    return 24;

                case PrimitiveType.PatchListWith25ControlPoints:
                    return 25;

                case PrimitiveType.PatchListWith26ControlPoints:
                    return 26;

                case PrimitiveType.PatchListWith27ControlPoints:
                    return 27;

                case PrimitiveType.PatchListWith28ControlPoints:
                    return 28;

                case PrimitiveType.PatchListWith29ControlPoints:
                    return 29;

                case PrimitiveType.PatchListWith30ControlPoints:
                    return 30;

                case PrimitiveType.PatchListWith31ControlPoints:
                    return 31;

                case PrimitiveType.PatchListWith32ControlPoints:
                    return 32;
                
                default:
                    throw new NotImplementedException();
            }
        }
    }
}