using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MiMap.Viewer.Core.Utilities;
using Veldrid;

namespace MiMap.Viewer.DesktopGL.Utilities
{
    public static class Spiral
    {

        public static IEnumerable<Point> FillRegionFromCenter(Rectangle region)
        {
            var c = region.Center();
            var cX = c.X;
            var cY = c.Y;
            var r = (int)Math.Max(region.Width, region.Height);

            var x = 0;
            var y = 0;
            var t = r;
            var dx = 0;
            var dy = -1;
            var p = new Point();
            
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool RegionContainsInclusive(this Rectangle region, Point point)
        => point.X >= region.X && point.X <= (region.X + region.Width) &&
           point.Y >= region.Y && point.Y <= (region.Y + region.Height);

    }
}