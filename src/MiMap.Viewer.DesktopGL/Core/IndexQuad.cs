using System;

namespace MiMap.Viewer.DesktopGL.Core
{
    public struct IndexQuad
    {
        public int TL;
        public int TR;
        public int BR;
        public int BL;

        public int this[int index]
        {
            get
            {
                if (index == 0) return TL;
                if (index == 1) return TR;
                if (index == 2) return BR;
                if (index == 3) return BL;
                
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index == 0)      TL = value;
                else if (index == 1) TR = value;
                else if (index == 2) BR = value;
                else if (index == 3) BL = value;
                else
                    throw new IndexOutOfRangeException();
                
            }
        }
        
        public int[] ToArray()
        {
            return new[] { TL, TR, BR, BL };
        }
    }
}