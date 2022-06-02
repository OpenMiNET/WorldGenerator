using ElementEngine;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public enum MiMapTilesWorldLayerType : int
    {
        Biome = 0,
        Downfall = 1,
        Temperature = 2,
        Height = 3,
    }

    public class MiMapTilesWorldChunk
    {
        private static readonly int MiMapTilesWorldLayerTypeCount = Enum.GetNames(typeof(MiMapTilesWorldLayerType)).Length;
        
        public MiMapTilesWorld World { get; set; }
        public Vector2I Position { get; set; }
        public MiMapTilesWorldLayer[] Layers { get; }

        public int TotalTiles => World.ChunkSize.X * World.ChunkSize.Y;

        public MiMapTilesWorldChunk(Vector2I position, MiMapTilesWorld world)
        {
            Position = position;
            World = world;
            Layers = new MiMapTilesWorldLayer[MiMapTilesWorldLayerTypeCount];
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i] = new MiMapTilesWorldLayer((MiMapTilesWorldLayerType)i, this);
            }
        }

        public void UpdateLayerTiles(MiMapTilesWorldLayerType type, int[] tiles)
        {
            Layers[(int)type].ResetTiles(tiles);
        }

        public void ClearTiles()
        {
            foreach (var layer in Layers)
                layer.ClearTiles();
        }

        public void ResetTiles()
        {
            foreach (var layer in Layers)
                layer.ResetTiles();
        }
    }
}