using ElementEngine;
using ElementEngine.ECS;

namespace MiMap.Viewer.Element.MiMapTiles
{
    // MiMapTilesRendererChunk

    public class MiMapTilesRenderer
    {
        public MiMapTilesWorld World { get; set; }
        public Texture2D Tilesheet { get; set; }
        public Entity Entity { get; }
        public Dictionary<Vector2I, MiMapTilesRendererChunk> Chunks { get; set; } = new Dictionary<Vector2I, MiMapTilesRendererChunk>();

        public MiMapTilesRenderer(MiMapTilesWorld world, Texture2D tilesheet, Entity entity)
        {
            World = world;
            Tilesheet = tilesheet;
            Entity = entity;
            
            World.ChunkAdded += WorldOnChunkAdded;

            foreach (var (_, chunk) in world.Chunks)
                Chunks.Add(chunk.Position, new MiMapTilesRendererChunk(chunk, tilesheet));
        }

        private void WorldOnChunkAdded(object sender, MiMapTilesWorldChunk e)
        {
            Chunks.Add(e.Position, new MiMapTilesRendererChunk(e, Tilesheet));
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
