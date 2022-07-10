using System.Runtime.CompilerServices;

namespace MiMap.Viewer.Core.Utilities
{
    public static class MathUtil
    {
        #region Clamp
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float v, float min, float max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(this double v, double min, double max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Clamp(this sbyte v, sbyte min, sbyte max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(this byte v, byte min, byte max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Clamp(this short v, short min, short max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp(this ushort v, ushort min, ushort max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(this int v, int min, int max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(this uint v, uint min, uint max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(this long v, long min, long max) => v < min ? min : v > max ? max : v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Clamp(this ulong v, ulong min, ulong max) => v < min ? min : v > max ? max : v;
        
        #endregion
    }
}