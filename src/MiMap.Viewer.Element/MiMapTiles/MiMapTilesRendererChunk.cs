using ElementEngine;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public class MiMapTilesRendererChunk
    {
        public MiMapTilesWorldChunk ChunkData { get; set; }
        public Rectangle ChunkRect;
        public TileBatch2D TileBatch { get; set; }

        public MiMapTilesRendererChunk(MiMapTilesWorldChunk chunkData, Texture2D tilesheet)
        {
            ChunkData = chunkData;
            ChunkRect = new Rectangle(
                ChunkData.Position * (ChunkData.World.TileSize * ChunkData.World.ChunkSize),
                ChunkData.World.TileSize * ChunkData.World.ChunkSize);

            TileBatch = new TileBatch2D(ChunkData.World.ChunkSize.X, ChunkData.World.ChunkSize.Y, ChunkData.World.TileSize.X, ChunkData.World.TileSize.Y, tilesheet, TileBatch2DWrapMode.None, ChunkData.World.TileAnimations);

            TileBatch.BeginBuild();

            foreach (var layer in ChunkData.Layers)
                BuildTilebatchLayer(layer);

            TileBatch.EndBuild();
        }

        public void BuildTilebatchLayer(MiMapTilesWorldLayer layer)
        {
            for (int y = 0; y < ChunkData.World.ChunkSize.Y; y++)
            {
                for (int x = 0; x < ChunkData.World.ChunkSize.X; x++)
                {
                    var tileID = layer.Tiles[x + ChunkData.World.ChunkSize.X * y];

                    if (tileID == 0)
                        continue;

                    TileBatch.SetTileAtPosition(x, y, tileID);
                }
            }

            TileBatch.EndLayer();
            layer.ClearTiles();
        }
    }
}