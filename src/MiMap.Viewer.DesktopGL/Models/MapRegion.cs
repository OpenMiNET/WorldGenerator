namespace MiMap.Viewer.DesktopGL
{
    public class MapRegion
    {
        public readonly int X;
        public readonly int Z;

        public readonly MapChunk[] Chunks;
        public bool IsComplete { get; internal set; }

        public MapRegion(int x, int z)
        {
            X = x;
            Z = z;
            Chunks = new MapChunk[32 * 32];
        }

        public void SetChunk(int cx, int cz, MapChunk chunk)
        {
            Chunks[GetIndex(cx, cz)] = chunk;
        }

        public MapChunk this[int cx, int cz]
        {
            get => Chunks[GetIndex(cx, cz)];
            set => Chunks[GetIndex(cx, cz)] = value;
        }

        private int GetIndex(int x, int z)
        {
            return ((x & 0x1F) * 32) + (z & 0x1F);
        }
    }
}