using ElementEngine;

namespace MiMap.Viewer.Element.MiMapTiles
{
    // MiMapTilesRendererChunk

    public class MiMapTilesRenderer
    {
        public MiMapTilesWorld World { get; set; }
        public Texture2D Tilesheet { get; set; }
        public Dictionary<Vector2I, MiMapTilesRendererChunk> Chunks { get; set; } = new Dictionary<Vector2I, MiMapTilesRendererChunk>();

        public MiMapTilesRenderer(MiMapTilesWorld world, Texture2D tilesheet)
        {
            World = world;
            Tilesheet = tilesheet;

            foreach (var (_, chunk) in world.Chunks)
                Chunks.Add(chunk.Position, new MiMapTilesRendererChunk(chunk, tilesheet));
        }

        public void Update(GameTimer gameTimer)
        {
            foreach (var (_, chunk) in Chunks)
                chunk.TileBatch.Update(gameTimer);
        }

        public void DrawLayers(int start, int end, Camera2D camera)
        {
            foreach (var (pos, chunk) in Chunks)
            {
                if (camera.ScaledView.Intersects(chunk.ChunkRect))
                    chunk.TileBatch.DrawLayers(start, end, camera, chunk.ChunkRect.LocationF);
            }
        } // DrawLayers

    } // MiMapTilesRenderer
}
