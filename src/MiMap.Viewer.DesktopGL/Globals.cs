using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MiMap.Viewer.DesktopGL
{
    public static class Globals
    {
        public static readonly Color[] BiomeColors;

        static Globals()
        {
            BiomeColors = new Color[byte.MaxValue];
            
            byte i = 0;
            BiomeColors[i++] = new Color(0xFF700000);
            BiomeColors[i++] = new Color(0xFF60B38D);
            BiomeColors[i++] = new Color(0xFF1894FA);
            BiomeColors[i++] = new Color(0xFF606060);
            BiomeColors[i++] = new Color(0xFF216605);
            BiomeColors[i++] = new Color(0xFF59660B);
            BiomeColors[i++] = new Color(0xFFB2F907);
            BiomeColors[i++] = new Color(0xFFFF0000);
            BiomeColors[i++] = new Color(0xFF0000FF);
            BiomeColors[i++] = new Color(0xFFFF8080);
            BiomeColors[i++] = new Color(0xFFA09090);
            BiomeColors[i++] = new Color(0xFFFFA0A0);
            BiomeColors[i++] = new Color(0xFFFFFFFF);
            BiomeColors[i++] = new Color(0xFFA0A0A0);
            BiomeColors[i++] = new Color(0xFFFF00FF);
            BiomeColors[i++] = new Color(0xFFFF00A0);
            BiomeColors[i++] = new Color(0xFF55DEFA);
            BiomeColors[i++] = new Color(0xFF125FD2);
            BiomeColors[i++] = new Color(0xFF1C5522);
            BiomeColors[i++] = new Color(0xFF333916);
            BiomeColors[i++] = new Color(0xFF9A7872);
            BiomeColors[i++] = new Color(0xFF097B53);
            BiomeColors[i++] = new Color(0xFF05422C);
            BiomeColors[i++] = new Color(0xFF178B62);
            BiomeColors[i++] = new Color(0xFF300000);
            BiomeColors[i++] = new Color(0xFF84A2A2);
            BiomeColors[i++] = new Color(0xFFC0F0FA);
            BiomeColors[i++] = new Color(0xFF447430);
            BiomeColors[i++] = new Color(0xFF325F1F);
            BiomeColors[i++] = new Color(0xFF1A5140);
            BiomeColors[i++] = new Color(0xFF4A5531);
            BiomeColors[i++] = new Color(0xFF363F24);
            BiomeColors[i++] = new Color(0xFF516659);
            BiomeColors[i++] = new Color(0xFF3E5F54);
            BiomeColors[i++] = new Color(0xFF507050);
            BiomeColors[i++] = new Color(0xFF5FB2BD);
            BiomeColors[i++] = new Color(0xFF649DA7);
            BiomeColors[i++] = new Color(0xFF1545D9);
            BiomeColors[i++] = new Color(0xFF6597B0);
            BiomeColors[i++] = new Color(0xFF658CCA);
            
            for (; i < BiomeColors.Length; i++)
            {
                BiomeColors[i] = Color.Transparent;
            }
        }
    }
}