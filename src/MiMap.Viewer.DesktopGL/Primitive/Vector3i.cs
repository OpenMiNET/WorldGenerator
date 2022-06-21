using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;

// ReSharper disable InconsistentNaming

namespace MiMap.Viewer.DesktopGL.Primitive
{
    public class Vector3iTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(Vector3i) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var vector3i = (Vector3i)value;
            if (destinationType == typeof(Vector3i))
            {
                return vector3i;
            }

            if (!(destinationType == typeof(string)))
                return base.ConvertTo(context, culture, value, destinationType);

            var strArray = new string[3]
            {
                vector3i.X.ToString("F0", culture),
                vector3i.Y.ToString("F0", culture),
                vector3i.Z.ToString("F0", culture)
            };
            return string.Join(culture.TextInfo.ListSeparator + " ", strArray);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            var valueType = value.GetType();
            var zero = Vector3i.Zero;
            if (!(valueType == typeof(string)))
                return base.ConvertFrom(context, culture, value);

            var strArray = ((string)value).Split(culture.TextInfo.ListSeparator.ToCharArray());
            zero.X = int.Parse(strArray[0], culture);
            zero.Y = int.Parse(strArray[1], culture);
            zero.Z = int.Parse(strArray[2], culture);

            return zero;
        }
    }

    /// <summary>Describes a 3D-vector.</summary>
    [TypeConverter(typeof(Vector3iTypeConverter))]
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Vector3i : IEquatable<Vector3i>
    {
        private static readonly Vector3i zero = new Vector3i(0, 0, 0);
        private static readonly Vector3i one = new Vector3i(1, 1, 1);
        private static readonly Vector3i unitX = new Vector3i(1, 0, 0);
        private static readonly Vector3i unitY = new Vector3i(0, 1, 0);
        private static readonly Vector3i unitZ = new Vector3i(0, 0, 1);
        private static readonly Vector3i up = new Vector3i(0, 1, 0);
        private static readonly Vector3i down = new Vector3i(0, -1, 0);
        private static readonly Vector3i right = new Vector3i(1, 0, 0);
        private static readonly Vector3i left = new Vector3i(-1, 0, 0);
        private static readonly Vector3i forward = new Vector3i(0, 0, -1);
        private static readonly Vector3i backward = new Vector3i(0, 0, 1);

        /// <summary>
        /// The x coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        [DataMember] public int X;

        /// <summary>
        /// The y coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        [DataMember] public int Y;

        /// <summary>
        /// The z coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        [DataMember] public int Z;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, 0, 0.
        /// </summary>
        public static Vector3i Zero => zero;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 1, 1, 1.
        /// </summary>
        public static Vector3i One => one;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 1, 0, 0.
        /// </summary>
        public static Vector3i UnitX => unitX;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, 1, 0.
        /// </summary>
        public static Vector3i UnitY => unitY;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, 0, 1.
        /// </summary>
        public static Vector3i UnitZ => unitZ;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, 1, 0.
        /// </summary>
        public static Vector3i Up => up;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, -1, 0.
        /// </summary>
        public static Vector3i Down => down;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 1, 0, 0.
        /// </summary>
        public static Vector3i Right => right;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components -1, 0, 0.
        /// </summary>
        public static Vector3i Left => left;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, 0, -1.
        /// </summary>
        public static Vector3i Forward => forward;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with components 0, 0, 1.
        /// </summary>
        public static Vector3i Backward => backward;

        internal string DebugDisplayString => X.ToString() + "  " + Y.ToString() + "  " + Z.ToString();

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3i(float x, float y, float z)
        {
            X = (int)x;
            Y = (int)y;
            Z = (int)z;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z set to the same value.
        /// </summary>
        /// <param name="value">The x, y and z coordinates in 3d-space.</param>
        public Vector3i(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y from <see cref="T:Microsoft.Xna.Framework.Vector3" />.
        /// </summary>
        /// <param name="value">The x, y and z coordinates in 3d-space.</param>
        public Vector3i(Vector3 value)
        {
            X = (int)value.X;
            Y = (int)value.Y;
            Z = (int)value.Z;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y from <see cref="T:Microsoft.Xna.Framework.Vector2" /> and Z from a scalar.
        /// </summary>
        /// <param name="value">The x and y coordinates in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector3i(Vector2 value, int z)
        {
            X = (int)value.X;
            Y = (int)value.Y;
            Z = z;
        }

        /// <summary>
        /// Performs vector addition on <paramref name="value1" /> and <paramref name="value2" />.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <returns>The result of the vector addition.</returns>
        public static Vector3i Add(Vector3i value1, Vector3i value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Performs vector addition on <paramref name="value1" /> and
        /// <paramref name="value2" />, storing the result of the
        /// addition in <paramref name="result" />.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <param name="result">The result of the vector addition.</param>
        public static void Add(ref Vector3i value1, ref Vector3i value2, out Vector3i result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
        /// </summary>
        /// <param name="value1">The first vector of 3d-triangle.</param>
        /// <param name="value2">The second vector of 3d-triangle.</param>
        /// <param name="value3">The third vector of 3d-triangle.</param>
        /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
        /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
        /// <returns>The cartesian translation of barycentric coordinates.</returns>
        public static Vector3i Barycentric(
            Vector3i value1,
            Vector3i value2,
            Vector3i value3,
            int amount1,
            int amount2)
        {
            return new Vector3i(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2), MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2));
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
        /// </summary>
        /// <param name="value1">The first vector of 3d-triangle.</param>
        /// <param name="value2">The second vector of 3d-triangle.</param>
        /// <param name="value3">The third vector of 3d-triangle.</param>
        /// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
        /// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
        /// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
        public static void Barycentric(
            ref Vector3i value1,
            ref Vector3i value2,
            ref Vector3i value3,
            int amount1,
            int amount2,
            out Vector3i result)
        {
            result.X = (int)MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
            result.Y = (int)MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
            result.Z = (int)MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of CatmullRom interpolation.</returns>
        public static Vector3i CatmullRom(
            Vector3i value1,
            Vector3i value2,
            Vector3i value3,
            Vector3i value4,
            int amount)
        {
            return new Vector3i((int)MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), (int)MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount), MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
        public static void CatmullRom(
            ref Vector3i value1,
            ref Vector3i value2,
            ref Vector3i value3,
            ref Vector3i value4,
            int amount,
            out Vector3i result)
        {
            result.X = (int)MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
            result.Y = (int)MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
            result.Z = (int)MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount);
        }

        /// <summary>
        /// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector3i" /> towards positive infinity.
        /// </summary>
        public void Ceiling()
        {
            X = (int)Math.Ceiling((double)X);
            Y = (int)Math.Ceiling((double)Y);
            Z = (int)Math.Ceiling((double)Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains members from another vector rounded towards positive infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public static Vector3i Ceiling(Vector3i value)
        {
            value.X = (int)Math.Ceiling((double)value.X);
            value.Y = (int)Math.Ceiling((double)value.Y);
            value.Z = (int)Math.Ceiling((double)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains members from another vector rounded towards positive infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        public static void Ceiling(ref Vector3i value, out Vector3i result)
        {
            result.X = (int)Math.Ceiling((double)value.X);
            result.Y = (int)Math.Ceiling((double)value.Y);
            result.Z = (int)Math.Ceiling((double)value.Z);
        }

        /// <summary>Clamps the specified value within a range.</summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector3i Clamp(Vector3i value1, Vector3i min, Vector3i max) => new Vector3i(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y), MathHelper.Clamp(value1.Z, min.Z, max.Z));

        /// <summary>Clamps the specified value within a range.</summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="result">The clamped value as an output parameter.</param>
        public static void Clamp(
            ref Vector3i value1,
            ref Vector3i min,
            ref Vector3i max,
            out Vector3i result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
            result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
        }

        /// <summary>Computes the cross product of two vectors.</summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product of two vectors.</returns>
        public static Vector3i Cross(Vector3i vector1, Vector3i vector2)
        {
            Cross(ref vector1, ref vector2, out vector1);
            return vector1;
        }

        /// <summary>Computes the cross product of two vectors.</summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <param name="result">The cross product of two vectors as an output parameter.</param>
        public static void Cross(ref Vector3i vector1, ref Vector3i vector2, out Vector3i result)
        {
            var num1 = (int)((double)vector1.Y * (double)vector2.Z - (double)vector2.Y * (double)vector1.Z);
            var num2 = (int)-((double)vector1.X * (double)vector2.Z - (double)vector2.X * (double)vector1.Z);
            var num3 = (int)((double)vector1.X * (double)vector2.Y - (double)vector2.X * (double)vector1.Y);
            result.X = num1;
            result.Y = num2;
            result.Z = num3;
        }

        /// <summary>Returns the distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between two vectors.</returns>
        public static int Distance(Vector3i value1, Vector3i value2)
        {
            int result;
            DistanceSquared(ref value1, ref value2, out result);
            return (int)Math.Sqrt((double)result);
        }

        /// <summary>Returns the distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The distance between two vectors as an output parameter.</param>
        public static void Distance(ref Vector3i value1, ref Vector3i value2, out int result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (int)Math.Sqrt((double)result);
        }

        /// <summary>Returns the squared distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between two vectors.</returns>
        public static int DistanceSquared(Vector3i value1, Vector3i value2) => (int)(((double)value1.X - (double)value2.X) * ((double)value1.X - (double)value2.X) + ((double)value1.Y - (double)value2.Y) * ((double)value1.Y - (double)value2.Y) + ((double)value1.Z - (double)value2.Z) * ((double)value1.Z - (double)value2.Z));

        /// <summary>Returns the squared distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The squared distance between two vectors as an output parameter.</param>
        public static void DistanceSquared(ref Vector3i value1, ref Vector3i value2, out int result) => result = (int)(((double)value1.X - (double)value2.X) * ((double)value1.X - (double)value2.X) + ((double)value1.Y - (double)value2.Y) * ((double)value1.Y - (double)value2.Y) + ((double)value1.Z - (double)value2.Z) * ((double)value1.Z - (double)value2.Z));

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector3i Divide(Vector3i value1, Vector3i value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector3i Divide(Vector3i value1, int divider)
        {
            var num = 1f / divider;
            value1.X = (int)(value1.X * num);
            value1.Y = (int)(value1.Y * num);
            value1.Z = (int)(value1.Z * num);
            return value1;
        }
        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector3i Divide(Vector3i value1, float divider)
        {
            var num = 1f / divider;
            value1.X = (int)(value1.X * num);
            value1.Y = (int)(value1.Y * num);
            value1.Z = (int)(value1.Z * num);
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
        public static void Divide(ref Vector3i value1, int divider, out Vector3i result)
        {
            var num = 1f / divider;
            result.X = (int)(value1.X * num);
            result.Y = (int)(value1.Y * num);
            result.Z = (int)(value1.Z * num);
        }
        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
        public static void Divide(ref Vector3i value1, float divider, out Vector3i result)
        {
            var num = 1f / divider;
            result.X =(int)(value1.X * num);
            result.Y =(int)(value1.Y * num);
            result.Z =(int)(value1.Z * num);
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The result of dividing the vectors as an output parameter.</param>
        public static void Divide(ref Vector3i value1, ref Vector3i value2, out Vector3i result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        /// <summary>Returns a dot product of two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The dot product of two vectors.</returns>
        public static int Dot(Vector3i value1, Vector3i value2) => (int)((double)value1.X * (double)value2.X + (double)value1.Y * (double)value2.Y + (double)value1.Z * (double)value2.Z);

        /// <summary>Returns a dot product of two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The dot product of two vectors as an output parameter.</param>
        public static void Dot(ref Vector3i value1, ref Vector3i value2, out int result) => result = (int)((double)value1.X * (double)value2.X + (double)value1.Y * (double)value2.Y + (double)value1.Z * (double)value2.Z);

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj) => obj is Vector3i vector3i && (double)X == (double)vector3i.X && (double)Y == (double)vector3i.Y && (double)Z == (double)vector3i.Z;

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Vector3i" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector3i other) => (double)X == (double)other.X && (double)Y == (double)other.Y && (double)Z == (double)other.Z;

        /// <summary>
        /// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector3i" /> towards negative infinity.
        /// </summary>
        public void Floor()
        {
            X = (int)Math.Floor((double)X);
            Y = (int)Math.Floor((double)Y);
            Z = (int)Math.Floor((double)Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains members from another vector rounded towards negative infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public static Vector3i Floor(Vector3i value)
        {
            value.X = (int)Math.Floor((double)value.X);
            value.Y = (int)Math.Floor((double)value.Y);
            value.Z = (int)Math.Floor((double)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains members from another vector rounded towards negative infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        public static void Floor(ref Vector3i value, out Vector3i result)
        {
            result.X = (int)Math.Floor((double)value.X);
            result.Y = (int)Math.Floor((double)value.Y);
            result.Z = (int)Math.Floor((double)value.Z);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public override int GetHashCode() => (X.GetHashCode() * 397 ^ Y.GetHashCode()) * 397 ^ Z.GetHashCode();

        /// <summary>
        /// Returns the length of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <returns>The length of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public int Length() => (int)Math.Sqrt((double)X * (double)X + (double)Y * (double)Y + (double)Z * (double)Z);

        /// <summary>
        /// Returns the squared length of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <returns>The squared length of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public int LengthSquared() => (int)((double)X * (double)X + (double)Y * (double)Y + (double)Z * (double)Z);

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector3i Lerp(Vector3i value1, Vector3i value2, int amount) => new Vector3i(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount), MathHelper.Lerp(value1.Z, value2.Z, amount));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void Lerp(
            ref Vector3i value1,
            ref Vector3i value2,
            int amount,
            out Vector3i result)
        {
            result.X = (int)MathHelper.Lerp(value1.X, value2.X, amount);
            result.Y = (int)MathHelper.Lerp(value1.Y, value2.Y, amount);
            result.Z = (int)MathHelper.Lerp(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector3i.Lerp(Microsoft.Xna.Framework.Vector3i,Microsoft.Xna.Framework.Vector3i,System.Single)" />.
        /// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector3i LerpPrecise(Vector3i value1, Vector3i value2, int amount) => new Vector3i(MathHelper.LerpPrecise(value1.X, value2.X, amount), MathHelper.LerpPrecise(value1.Y, value2.Y, amount), MathHelper.LerpPrecise(value1.Z, value2.Z, amount));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector3i.Lerp(Microsoft.Xna.Framework.Vector3i@,Microsoft.Xna.Framework.Vector3i@,System.Single,Microsoft.Xna.Framework.Vector3i@)" />.
        /// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void LerpPrecise(
            ref Vector3i value1,
            ref Vector3i value2,
            int amount,
            out Vector3i result)
        {
            result.X = (int)MathHelper.LerpPrecise(value1.X, value2.X, amount);
            result.Y = (int)MathHelper.LerpPrecise(value1.Y, value2.Y, amount);
            result.Z = (int)MathHelper.LerpPrecise(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with maximal values from the two vectors.</returns>
        public static Vector3i Max(Vector3i value1, Vector3i value2) => new Vector3i(MathHelper.Max(value1.X, value2.X), MathHelper.Max(value1.Y, value2.Y), MathHelper.Max(value1.Z, value2.Z));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with maximal values from the two vectors as an output parameter.</param>
        public static void Max(ref Vector3i value1, ref Vector3i value2, out Vector3i result)
        {
            result.X = MathHelper.Max(value1.X, value2.X);
            result.Y = MathHelper.Max(value1.Y, value2.Y);
            result.Z = MathHelper.Max(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with minimal values from the two vectors.</returns>
        public static Vector3i Min(Vector3i value1, Vector3i value2) => new Vector3i(MathHelper.Min(value1.X, value2.X), MathHelper.Min(value1.Y, value2.Y), MathHelper.Min(value1.Z, value2.Z));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector3i" /> with minimal values from the two vectors as an output parameter.</param>
        public static void Min(ref Vector3i value1, ref Vector3i value2, out Vector3i result)
        {
            result.X = MathHelper.Min(value1.X, value2.X);
            result.Y = MathHelper.Min(value1.Y, value2.Y);
            result.Z = MathHelper.Min(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The result of the vector multiplication.</returns>
        public static Vector3i Multiply(Vector3i value1, Vector3i value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>The result of the vector multiplication with a scalar.</returns>
        public static Vector3i Multiply(Vector3i value1, int scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Vector3i value1, int scaleFactor, out Vector3i result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The result of the vector multiplication as an output parameter.</param>
        public static void Multiply(ref Vector3i value1, ref Vector3i value2, out Vector3i result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The result of the vector inversion.</returns>
        public static Vector3i Negate(Vector3i value)
        {
            value = new Vector3i(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The result of the vector inversion as an output parameter.</param>
        public static void Negate(ref Vector3i value, out Vector3i result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }

        /// <summary>
        /// Turns this <see cref="T:Microsoft.Xna.Framework.Vector3i" /> to a unit vector with the same direction.
        /// </summary>
        public void Normalize()
        {
            var num = 1f / (int)Math.Sqrt((double)X * (double)X + (double)Y * (double)Y + (double)Z * (double)Z);
            X =(int)(X * num);
            Y =(int)(Y * num);
            Z =(int)(Z * num);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a normalized values from another vector.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>Unit vector.</returns>
        public static Vector3i Normalize(Vector3i value)
        {
            var num = 1f / (int)Math.Sqrt((double)value.X * (double)value.X + (double)value.Y * (double)value.Y + (double)value.Z * (double)value.Z);
            return new Vector3i(value.X * num, value.Y * num, value.Z * num);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a normalized values from another vector.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">Unit vector as an output parameter.</param>
        public static void Normalize(ref Vector3i value, out Vector3i result)
        {
            var num = 1f / (int)Math.Sqrt((double)value.X * (double)value.X + (double)value.Y * (double)value.Y + (double)value.Z * (double)value.Z);
            result.X = (int)(value.X * num);
            result.Y = (int)(value.Y * num);
            result.Z = (int)(value.Z * num);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <returns>Reflected vector.</returns>
        public static Vector3i Reflect(Vector3i vector, Vector3i normal)
        {
            var num = (int)((double)vector.X * (double)normal.X + (double)vector.Y * (double)normal.Y + (double)vector.Z * (double)normal.Z);
            Vector3i vector3I;
            vector3I.X = (int)(vector.X - 2f * normal.X * num);
            vector3I.Y = (int)(vector.Y - 2f * normal.Y * num);
            vector3I.Z = (int)(vector.Z - 2f * normal.Z * num);
            return vector3I;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <param name="result">Reflected vector as an output parameter.</param>
        public static void Reflect(ref Vector3i vector, ref Vector3i normal, out Vector3i result)
        {
            var num = (int)((double)vector.X * (double)normal.X + (double)vector.Y * (double)normal.Y + (double)vector.Z * (double)normal.Z);
            result.X = (int)(vector.X - 2f * normal.X * num);
            result.Y = (int)(vector.Y - 2f * normal.Y * num);
            result.Z = (int)(vector.Z - 2f * normal.Z * num);
        }

        /// <summary>
        /// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector3i" /> towards the nearest integer value.
        /// </summary>
        public void Round()
        {
            X = (int)Math.Round((double)X);
            Y = (int)Math.Round((double)Y);
            Z = (int)Math.Round((double)Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains members from another vector rounded to the nearest integer value.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public static Vector3i Round(Vector3i value)
        {
            value.X = (int)Math.Round((double)value.X);
            value.Y = (int)Math.Round((double)value.Y);
            value.Z = (int)Math.Round((double)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains members from another vector rounded to the nearest integer value.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        public static void Round(ref Vector3i value, out Vector3i result)
        {
            result.X = (int)Math.Round((double)value.X);
            result.Y = (int)Math.Round((double)value.Y);
            result.Z = (int)Math.Round((double)value.Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector3i" /> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <returns>The result of the vector subtraction.</returns>
        public static Vector3i Subtract(Vector3i value1, Vector3i value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector3i" /> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="result">The result of the vector subtraction as an output parameter.</param>
        public static void Subtract(ref Vector3i value1, ref Vector3i value2, out Vector3i result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector3i" /> in the format:
        /// {X:[<see cref="F:Microsoft.Xna.Framework.Vector3i.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Vector3i.Y" />] Z:[<see cref="F:Microsoft.Xna.Framework.Vector3i.Z" />]}
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder(32);
            stringBuilder.Append("{X:");
            stringBuilder.Append(X);
            stringBuilder.Append(" Y:");
            stringBuilder.Append(Y);
            stringBuilder.Append(" Z:");
            stringBuilder.Append(Z);
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
        /// </summary>
        /// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public static Vector3i Transform(Vector3i position, Matrix matrix)
        {
            Transform(ref position, ref matrix, out position);
            return position;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
        /// </summary>
        /// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector3i" /> as an output parameter.</param>
        public static void Transform(ref Vector3i position, ref Matrix matrix, out Vector3i result)
        {
            var num1 = (int)((double)position.X * (double)matrix.M11 + (double)position.Y * (double)matrix.M21 + (double)position.Z * (double)matrix.M31) + matrix.M41;
            var num2 = (int)((double)position.X * (double)matrix.M12 + (double)position.Y * (double)matrix.M22 + (double)position.Z * (double)matrix.M32) + matrix.M42;
            var num3 = (int)((double)position.X * (double)matrix.M13 + (double)position.Y * (double)matrix.M23 + (double)position.Z * (double)matrix.M33) + matrix.M43;
            result.X = (int)num1;
            result.Y = (int)num2;
            result.Z = (int)num3;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />, representing the rotation.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
        /// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</returns>
        public static Vector3i Transform(Vector3i value, Quaternion rotation)
        {
            Vector3i result;
            Transform(ref value, ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />, representing the rotation.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" />.</param>
        /// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
        /// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector3i" /> as an output parameter.</param>
        public static void Transform(ref Vector3i value, ref Quaternion rotation, out Vector3i result)
        {
            var num1 = (int)(2.0 * ((double)rotation.Y * (double)value.Z - (double)rotation.Z * (double)value.Y));
            var num2 = (int)(2.0 * ((double)rotation.Z * (double)value.X - (double)rotation.X * (double)value.Z));
            var num3 = (int)(2.0 * ((double)rotation.X * (double)value.Y - (double)rotation.Y * (double)value.X));
            result.X = (int)((double)value.X + (double)num1 * (double)rotation.W + ((double)rotation.Y * (double)num3 - (double)rotation.Z * (double)num2));
            result.Y = (int)((double)value.Y + (double)num2 * (double)rotation.W + ((double)rotation.Z * (double)num1 - (double)rotation.X * (double)num3));
            result.Z = (int)((double)value.Z + (double)num3 * (double)rotation.W + ((double)rotation.X * (double)num2 - (double)rotation.Y * (double)num1));
        }

        /// <summary>
        /// Apply transformation on vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector3i" /> should be written.</param>
        /// <param name="length">The number of vectors to be transformed.</param>
        public static void Transform(
            Vector3i[] sourceArray,
            int sourceIndex,
            ref Matrix matrix,
            Vector3i[] destinationArray,
            int destinationIndex,
            int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
            for (var index = 0; index < length; ++index)
            {
                var source = sourceArray[sourceIndex + index];
                destinationArray[destinationIndex + index] = new Vector3i((int)((double)source.X * (double)matrix.M11 + (double)source.Y * (double)matrix.M21 + (double)source.Z * (double)matrix.M31) + matrix.M41, (int)((double)source.X * (double)matrix.M12 + (double)source.Y * (double)matrix.M22 + (double)source.Z * (double)matrix.M32) + matrix.M42, (int)((double)source.X * (double)matrix.M13 + (double)source.Y * (double)matrix.M23 + (double)source.Z * (double)matrix.M33) + matrix.M43);
            }
        }

        /// <summary>
        /// Apply transformation on vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector3i" /> should be written.</param>
        /// <param name="length">The number of vectors to be transformed.</param>
        public static void Transform(
            Vector3i[] sourceArray,
            int sourceIndex,
            ref Quaternion rotation,
            Vector3i[] destinationArray,
            int destinationIndex,
            int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
            for (var index = 0; index < length; ++index)
            {
                var source = sourceArray[sourceIndex + index];
                var num1 = (int)(2.0 * ((double)rotation.Y * (double)source.Z - (double)rotation.Z * (double)source.Y));
                var num2 = (int)(2.0 * ((double)rotation.Z * (double)source.X - (double)rotation.X * (double)source.Z));
                var num3 = (int)(2.0 * ((double)rotation.X * (double)source.Y - (double)rotation.Y * (double)source.X));
                destinationArray[destinationIndex + index] = new Vector3i((int)((double)source.X + (double)num1 * (double)rotation.W + ((double)rotation.Y * (double)num3 - (double)rotation.Z * (double)num2)), (int)((double)source.Y + (double)num2 * (double)rotation.W + ((double)rotation.Z * (double)num1 - (double)rotation.X * (double)num3)), (int)((double)source.Z + (double)num3 * (double)rotation.W + ((double)rotation.X * (double)num2 - (double)rotation.Y * (double)num1)));
            }
        }

        /// <summary>
        /// Apply transformation on all vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void Transform(
            Vector3i[] sourceArray,
            ref Matrix matrix,
            Vector3i[] destinationArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");
            for (var index = 0; index < sourceArray.Length; ++index)
            {
                var source = sourceArray[index];
                destinationArray[index] = new Vector3i((int)((double)source.X * (double)matrix.M11 + (double)source.Y * (double)matrix.M21 + (double)source.Z * (double)matrix.M31) + matrix.M41, (int)((double)source.X * (double)matrix.M12 + (double)source.Y * (double)matrix.M22 + (double)source.Z * (double)matrix.M32) + matrix.M42, (int)((double)source.X * (double)matrix.M13 + (double)source.Y * (double)matrix.M23 + (double)source.Z * (double)matrix.M33) + matrix.M43);
            }
        }

        /// <summary>
        /// Apply transformation on all vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void Transform(
            Vector3i[] sourceArray,
            ref Quaternion rotation,
            Vector3i[] destinationArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");
            for (var index = 0; index < sourceArray.Length; ++index)
            {
                var source = sourceArray[index];
                var num1 = (int)(2.0 * ((double)rotation.Y * (double)source.Z - (double)rotation.Z * (double)source.Y));
                var num2 = (int)(2.0 * ((double)rotation.Z * (double)source.X - (double)rotation.X * (double)source.Z));
                var num3 = (int)(2.0 * ((double)rotation.X * (double)source.Y - (double)rotation.Y * (double)source.X));
                destinationArray[index] = new Vector3i((int)((double)source.X + (double)num1 * (double)rotation.W + ((double)rotation.Y * (double)num3 - (double)rotation.Z * (double)num2)), (int)((double)source.Y + (double)num2 * (double)rotation.W + ((double)rotation.Z * (double)num1 - (double)rotation.X * (double)num3)), (int)((double)source.Z + (double)num3 * (double)rotation.W + ((double)rotation.X * (double)num2 - (double)rotation.Y * (double)num1)));
            }
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a transformation of the specified normal by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
        /// </summary>
        /// <param name="normal">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> which represents a normal vector.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <returns>Transformed normal.</returns>
        public static Vector3i TransformNormal(Vector3i normal, Matrix matrix)
        {
            TransformNormal(ref normal, ref matrix, out normal);
            return normal;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3i" /> that contains a transformation of the specified normal by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
        /// </summary>
        /// <param name="normal">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> which represents a normal vector.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <param name="result">Transformed normal as an output parameter.</param>
        public static void TransformNormal(ref Vector3i normal, ref Matrix matrix, out Vector3i result)
        {
            var num1 = (int)((double)normal.X * (double)matrix.M11 + (double)normal.Y * (double)matrix.M21 + (double)normal.Z * (double)matrix.M31);
            var num2 = (int)((double)normal.X * (double)matrix.M12 + (double)normal.Y * (double)matrix.M22 + (double)normal.Z * (double)matrix.M32);
            var num3 = (int)((double)normal.X * (double)matrix.M13 + (double)normal.Y * (double)matrix.M23 + (double)normal.Z * (double)matrix.M33);
            result.X = num1;
            result.Y = num2;
            result.Z = num3;
        }

        /// <summary>
        /// Apply transformation on normals within array of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="sourceIndex">The starting index of transformation in the source array.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <param name="destinationArray">Destination array.</param>
        /// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector3i" /> should be written.</param>
        /// <param name="length">The number of normals to be transformed.</param>
        public static void TransformNormal(
            Vector3i[] sourceArray,
            int sourceIndex,
            ref Matrix matrix,
            Vector3i[] destinationArray,
            int destinationIndex,
            int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (sourceArray.Length < sourceIndex + length)
                throw new ArgumentException("Source array length is lesser than sourceIndex + length");
            if (destinationArray.Length < destinationIndex + length)
                throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
            for (var index = 0; index < length; ++index)
            {
                var source = sourceArray[sourceIndex + index];
                destinationArray[destinationIndex + index] = new Vector3i((int)((double)source.X * (double)matrix.M11 + (double)source.Y * (double)matrix.M21 + (double)source.Z * (double)matrix.M31), (int)((double)source.X * (double)matrix.M12 + (double)source.Y * (double)matrix.M22 + (double)source.Z * (double)matrix.M32), (int)((double)source.X * (double)matrix.M13 + (double)source.Y * (double)matrix.M23 + (double)source.Z * (double)matrix.M33));
            }
        }

        /// <summary>
        /// Apply transformation on all normals within array of <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
        /// </summary>
        /// <param name="sourceArray">Source array.</param>
        /// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
        /// <param name="destinationArray">Destination array.</param>
        public static void TransformNormal(
            Vector3i[] sourceArray,
            ref Matrix matrix,
            Vector3i[] destinationArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException("Destination array length is lesser than source array length");
            for (var index = 0; index < sourceArray.Length; ++index)
            {
                var source = sourceArray[index];
                destinationArray[index] = new Vector3i((int)((double)source.X * (double)matrix.M11 + (double)source.Y * (double)matrix.M21 + (double)source.Z * (double)matrix.M31), (int)((double)source.X * (double)matrix.M12 + (double)source.Y * (double)matrix.M22 + (double)source.Z * (double)matrix.M32), (int)((double)source.X * (double)matrix.M13 + (double)source.Y * (double)matrix.M23 + (double)source.Z * (double)matrix.M33));
            }
        }

        /// <summary>
        /// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector3i" /> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector3i" /> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector3i" /> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector3i value1, Vector3i value2) => (double)value1.X == (double)value2.X && (double)value1.Y == (double)value2.Y && (double)value1.Z == (double)value2.Z;

        /// <summary>
        /// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector3i" /> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector3i" /> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector3i" /> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Vector3i value1, Vector3i value2) => !(value1 == value2);

        /// <summary>Adds two vectors.</summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector3i operator +(Vector3i value1, Vector3i value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector3i operator -(Vector3i value)
        {
            value = new Vector3i(-value.X, -value.Y, -value.Z);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> from a <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector3i operator -(Vector3i value1, Vector3i value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector3i operator *(Vector3i value1, Vector3i value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>Multiplies the components of vector by a scalar.</summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector3i operator *(Vector3i value, float scaleFactor)
        {
            value.X =(int)(value.X* scaleFactor);
            value.Y =(int)(value.Y* scaleFactor);
            value.Z =(int)(value.Z* scaleFactor);
            return value;
        }

        /// <summary>Multiplies the components of vector by a scalar.</summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector3i operator *(int scaleFactor, Vector3i value)
        {
            value.X *= scaleFactor;
            value.Y *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector3i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector3i operator /(Vector3i value1, Vector3i value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3i" /> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector3i operator /(Vector3i value1, float divider)
        {
            var num = 1f / divider;
            value1.X =(int)(value1.X* num);
            value1.Y =(int)(value1.Y* num);
            value1.Z =(int)(value1.Z* num);
            return value1;
        }
        
        public static implicit operator Vector3 (Vector3i v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }        
        public static explicit operator Vector3i (Vector3 v)
        {
            return new Vector3i(v.X, v.Y, v.Z);
        }
        
    }
}