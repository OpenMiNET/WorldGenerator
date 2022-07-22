using MiMap.Viewer.DesktopGL.Primitive;

namespace MiMap.Viewer.DesktopGL.Models
{
    public interface IMapChunkData
    {
        Vector3i ChunkSize { get; }
        
        int? this[int x, int y, int z] { get; }
    }
}