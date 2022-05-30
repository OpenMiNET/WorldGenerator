using System;
using System.Numerics;

namespace OpenAPI.WorldGenerator.Utils.Noise.Cellular
{
    public class VoronoiResult
    {
        public float ShortestDistance { get; set; } = 32000000.0f;
        public float NextDistance { get; set; } = 32000000.0f;
        public float ClosestX { get; set; } = 32000000.0f;
        public float ClosestZ { get; set; } = 32000000.0f;
        
        public float BorderValue => ShortestDistance / NextDistance;
        public float InteriorValue => (NextDistance - ShortestDistance) / NextDistance;

        public VoronoiResult(VoronoiSettings settings)
        {
            ShortestDistance = settings.ShortestDistance;
            NextDistance = settings.NextDistance;
            ClosestX = settings.ClosestX;
            ClosestZ = settings.ClosestZ;
        }
        
        public Vector2 ToLength(Vector2 toMap, float radius) {
            double distance = Distance(toMap, this.ClosestX, this.ClosestZ);
            double xDist = toMap.X - this.ClosestX;
            double zDist = toMap.Y - this.ClosestZ;
            xDist *= radius / distance;
            zDist *= radius / distance;
            return new Vector2((float) (this.ClosestX + xDist), (float) (this.ClosestZ + zDist));
        }

        private static float Distance(Vector2 map, float px, float py)
        {
            px -= map.X;
            py -= map.Y;
            return MathF.Sqrt(px * px + py * py);
        }

        public void Evaluate(Vector2[] points, float x, float z) {
            foreach (Vector2 point in points) {
                float distance = Distance(point, x, z);
                if (distance < this.ShortestDistance) {
                    this.NextDistance = this.ShortestDistance;
                    this.ShortestDistance = distance;
                    this.ClosestX = point.X;
                    this.ClosestZ = point.Y;
                }
                else if (distance < this.NextDistance) {
                    this.NextDistance = distance;
                }
            }
        }
    }
}