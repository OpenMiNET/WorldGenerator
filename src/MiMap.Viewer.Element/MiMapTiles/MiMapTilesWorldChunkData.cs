using ElementEngine;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public class MiMapTilesWorldChunkData
    {
        public Vector2I Position { get; set; }
        public List<MiMapTilesWorldLayerData> Layers { get; set; }

    }
}