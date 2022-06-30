using System;

namespace OpenMiNET.Noise.Maths
{
	public static class FastMath
	{
		public static int Floor(float value)
		{
			return (int) MathF.Floor(value);
		}
		
		public static int Floor(double value)
		{
			return (int) Math.Floor(value);
		}
	}
}