namespace MiMap.Viewer.Element.MiMapTiles
{
    public class MiMapTilesWorldLayer
    {
        public MiMapTilesWorldChunk Chunk { get; }

        public MiMapTilesWorldLayerType Type { get; }
        public int[] Tiles { get; set; }

        public bool TilesLoaded => Tiles != null;

        public MiMapTilesWorldLayer(MiMapTilesWorldLayerType type, MiMapTilesWorldChunk chunk)
        {
            Type = type;
            Tiles = null;
            Chunk = chunk;
        }
        public MiMapTilesWorldLayer(MiMapTilesWorldLayerType type, int[] tiles, MiMapTilesWorldChunk chunk) : this(type, chunk)
        {
            Tiles = tiles;
        }

        public void ClearTiles()
        {
            Tiles = null;
        }

        public void ResetTiles(int[] newTiles)
        {
            if (newTiles.Length != Chunk.TotalTiles)
                throw new IndexOutOfRangeException(nameof(newTiles));
            
            Tiles = newTiles;
        }
        
        public void ResetTiles()
        {
            Tiles = new int[Chunk.TotalTiles];
            for (var i = 0; i < Chunk.TotalTiles; i++)
                Tiles[i] = MiMapTilesWorld.BLANK_TILE;
        }

    }
}