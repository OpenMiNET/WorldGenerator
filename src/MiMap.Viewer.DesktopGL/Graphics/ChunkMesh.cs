using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using gs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Core;
using MiMap.Viewer.DesktopGL.Primitive;
using MiNET.Utils.Vectors;
using MiNET.Worlds;

namespace MiMap.Viewer.DesktopGL.Graphics
{
    public class ChunkMesh : MapChunk, IDisposable
    {
        public static bool OnlyLoadWhenAllNeighborChunks { get; set; } = false;
        public Matrix World { get; private set; }

        public bool HasAllMeshNeighbors
        {
            get => _hasAllMeshNeighbors;
            internal set => _hasAllMeshNeighbors = value;
        }

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private int _primitiveCount;

        private bool _hasAllMeshNeighbors = false;
        public bool Initialized { get; private set; } = false;
        private IRawMesh _mesh;
        private PrimitiveType _primitiveType;

        public ChunkMesh(ChunkCoordinates position) : this(new Vector2i(position.X << 4, position.Z << 4), 16)
        {
        }

        public ChunkMesh(Vector2i position, int sizeXZ) : base(position, sizeXZ)
        {
            World = Matrix.Identity
                    * Matrix.CreateTranslation(new Vector3(position.X, 0f, position.Z));
        }

        internal void UpdateMesh(IRawMesh mesh)
        {
            Initialized = false;
            _mesh = mesh;
        }

        public void Reload(GraphicsDevice graphicsDevice)
        {
            if (OnlyLoadWhenAllNeighborChunks && !HasAllMeshNeighbors) 
                return;
            
            if (_mesh != null)
            {
                Initialized = false;
                
                _vertexBuffer?.Dispose();
                _vertexBuffer = null;
                _indexBuffer?.Dispose();
                _indexBuffer = null;

                _mesh.CreateBuffers(graphicsDevice, out _vertexBuffer, out _indexBuffer, out _primitiveCount, out _primitiveType);
                _mesh = null;
                Initialized = true;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            if (!Initialized)
            {
                //if (_mesh == default)
                    return;

                //Reload(graphicsDevice);
            }

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;
            graphicsDevice.DrawIndexedPrimitives(_primitiveType, 0, 0, _primitiveCount);
        }

        public void Dispose()
        {
            Initialized = false;
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}