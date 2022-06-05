using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;

namespace MiMap.Viewer.DesktopGL.Utilities
{
    public static class Spiral
    {
        public static IEnumerable<Point> FillRegionFromCenter(Rectangle region)
        {
            var cX = region.Center.X;
            var cY = region.Center.Y;
            var r = (int)Math.Max(region.Width, region.Height);

            var x = 0;
            var y = 0;
            var t = r;
            var dx = 0;
            var dy = -1;
            var p = Point.Zero;

            for (var i = 0; i < (r * r); i++)
            {
                if ((-r / 2 <= x) && (x <= r / 2) && (-r / 2 <= y) && (y <= r / 2))
                {
                    p = new Point(cX + x, cY + y);
                    if (region.RegionContainsInclusive(p))
                        yield return p;
                }

                if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                {
                    t = dx;
                    dx = -dy;
                    dy = t;
                }

                x += dx;
                y += dy;
            }
        }

        public static IEnumerable<Point> FillRegionUniform(Rectangle region)
        {
            ;
            var maxX = region.X + region.Width;
            var maxY = region.Y + region.Height;

            var p = Point.Zero;

            for (var x = region.X; x <= maxX; x++)
            for (var y = region.Y; y <= maxY; y++)
            {
                p = new Point(x, y);
                yield return p;
            }
        }

        public static IEnumerable<Point> FillRegionRandom(Rectangle region)
        {
            var allPoints = FillRegionUniform(region).ToList();
            allPoints.Shuffle();
            return allPoints;
        }

        private static bool RegionContainsInclusive(this Rectangle region, Point point)
            => point.X >= region.X && point.X <= (region.X + region.Width) &&
               point.Y >= region.Y && point.Y <= (region.Y + region.Height);
    }
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom => 
            Local ??= new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId));
    }
}