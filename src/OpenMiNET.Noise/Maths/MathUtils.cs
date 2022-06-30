using System;

namespace OpenMiNET.Noise.Maths
{
    public static class MathUtils
    {
        public const float Pi = 3.1415927f;
        public const float Sqrt2 = 1.4142135f;
        public const float Sqrt3 = 1.7320508f;
        public const float Sqrt5 = 2.236068f;
        public const float Deg2Rad = 0.017453292f;
        public const float Rad2Deg = 57.29578f;
        public static float SCurve3(float a) => (float) ((double) a * (double) a * (3.0 - 2.0 * (double) a));
        public static float SCurve5(float a) => (float) ((double) a * (double) a * (double) a * ((double) a * ((double) a * 6.0 - 15.0) + 10.0));
        
        public static byte[] UnpackLittleUint32(int value, ref byte[] buffer)
        {
            if (buffer.Length < 4)
                Array.Resize<byte>(ref buffer, 4);
            buffer[0] = (byte) (value & (int) byte.MaxValue);
            buffer[1] = (byte) ((value & 65280) >> 8);
            buffer[2] = (byte) ((value & 16711680) >> 16);
            buffer[3] = (byte) (((long) value & 4278190080L) >> 24);
            return buffer;
        }
        
        public static float Lerp(float a, float b, float f)
        {
            //f = f*f;
            return a + f * (b - a);
        }

        public static double Clamp01(double value)
        {
            return Math.Clamp(value, 0d, 1d);
        }
        
        public static float Clamp01(float value)
        {
            return Math.Clamp(value, 0f, 1f);
        }
        
        public static float ConvertRange(
            float originalStart, float originalEnd, // original range
            float newStart, float newEnd, // desired range
            float value) // value to convert
        {
            //return ((value - originalStart) / (originalEnd - originalStart))
           //     * (newEnd - newStart) + newStart;
            
            float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
            return (newStart + ((value - originalStart) * scale));
        }
        
        private static float Lerp(float t, float x1, float x2, float q00, float q01)
        {
            return ((x2 - t) / (x2 - x1)) * q00 + ((t - x1) / (x2 - x1)) * q01;
        }

        public static float BilinearLerp(float x, float y, float q11, float q12, float q21, float q22, float x1, float x2,
            float y1, float y2)
        {
            float r1 = Lerp(x, x1, x2, q11, q21);
            float r2 = Lerp(x, x1, x2, q12, q22);

            return Lerp(y, y1, y2, r1, r2);
        }
        
        public static float Cmr(float p0, float p1, float p2, float p3, float t)
        {
            float a = 2f * p1;
            float b = p2 - p0;
            float c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            float d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            float pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }

        public static float BilinearCmr(float x, float y, float q11, float q12, float q21, float q22, float x1,
            float x2, float y1, float y2)
        {
            var xAlpha = (x2 - x) / (x2 - x1);
            var yAlpha = (y2 - y) / (y2 - y1);
            float r1 = Cmr(q11, q21, q11, q21, xAlpha);
            float r2 = Cmr(q12, q22, q12, q22, xAlpha);

            return Cmr(r1, r2, r1, r2, yAlpha);
        }

        public static float TrilinearCmr(float x, float y, float z, float q000, float q001, float q010, float q011,
            float q100, float q101, float q110, float q111, float x1, float x2, float y1, float y2, float z1, float z2)
        {
            var xAlpha = (x2 - x) / (x2 - x1);
            var yAlpha = (y2 - y) / (y2 - y1);
            var zAlpha = (z2 - z) / (z2 - z1);

            float x00 = Cmr(q000, q100, q000, q100, xAlpha);
            float x10 = Cmr(q010, q110, q010, q110, xAlpha);
            float x01 = Cmr(q001, q101, q001, q101, xAlpha);
            float x11 = Cmr(q011, q111, q011, q111, xAlpha);
            float r0 = Cmr(x00, x01, x00, x01, yAlpha);
            float r1 = Cmr(x10, x11, x10, x11, yAlpha);
            return Cmr(z1, z2, r0, r1, zAlpha);
        }

    }
}