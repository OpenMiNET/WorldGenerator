using ElementEngine;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public class MiMapTilesWorldData
    {
        public string Name { get; set; }
        public string TilesheetPath { get; set; }
        public string TilesheetName { get; set; }
        public Vector2I TileSize { get; set; }
        public Vector2I ChunkSize { get; set; }

        public Dictionary<string, MiMapTilesWorldChunkData> SavedChunks { get; set; }
        public Dictionary<int, TileAnimation> TileAnimations { get; set; }
    }
}