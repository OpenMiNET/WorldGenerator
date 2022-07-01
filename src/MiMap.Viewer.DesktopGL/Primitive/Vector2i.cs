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
    public class Vector2iTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(Vector2i) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var Vector2i = (Vector2i)value;
            if (destinationType == typeof(Vector2i))
            {
                return Vector2i;
            }

            if (!(destinationType == typeof(string)))
                return base.ConvertTo(context, culture, value, destinationType);

            var strArray = new string[2]
            {
                Vector2i.X.ToString("F0", culture),
                Vector2i.Z.ToString("F0", culture)
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
            var zero = Vector2i.Zero;
            if (!(valueType == typeof(string)))
                return base.ConvertFrom(context, culture, value);

            var strArray = ((string)value).Split(culture.TextInfo.ListSeparator.ToCharArray());
            zero.X = int.Parse(strArray[0], culture);
            zero.Z = int.Parse(strArray[1], culture);

            return zero;
        }
    }

    /// <summary>Describes a 3D-vector.</summary>
    [TypeConverter(typeof(Vector2iTypeConverter))]
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Vector2i : IEquatable<Vector2i>
    {
        private static readonly Vector2i zero = new Vector2i(0, 0);
        private static readonly Vector2i one = new Vector2i(1, 1);
        private static readonly Vector2i unitX = new Vector2i(1, 0);
        private static readonly Vector2i unitZ = new Vector2i(0, 1);
        private static readonly Vector2i right = new Vector2i(1, 0);
        private static readonly Vector2i left = new Vector2i(-1, 0);
        private static readonly Vector2i forward = new Vector2i(0, -1);
        private static readonly Vector2i backward = new Vector2i(0, 1);

        /// <summary>
        /// The x coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        [DataMember] public int X;

        /// <summary>
        /// The z coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        [DataMember] public int Z;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 0, 0, 0.
        /// </summary>
        public static Vector2i Zero => zero;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 1, 1, 1.
        /// </summary>
        public static Vector2i One => one;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 1, 0, 0.
        /// </summary>
        public static Vector2i UnitX => unitX;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 0, 0, 1.
        /// </summary>
        public static Vector2i UnitZ => unitZ;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 1, 0, 0.
        /// </summary>
        public static Vector2i Right => right;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components -1, 0, 0.
        /// </summary>
        public static Vector2i Left => left;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 0, 0, -1.
        /// </summary>
        public static Vector2i Forward => forward;

        /// <summary>
        /// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with components 0, 0, 1.
        /// </summary>
        public static Vector2i Backward => backward;

        internal string DebugDisplayString => X.ToString() + "  " + Z.ToString();

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector2i(int x, int z)
        {
            X = x;
            Z = z;
        }
        /// <summary>
        /// Constructs a 3d vector with X, Y and Z from three values.
        /// </summary>
        /// <param name="x">The x coordinate in 3d-space.</param>
        /// <param name="y">The y coordinate in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector2i(float x, float z)
        {
            X = (int)x;
            Z = (int)z;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y and Z set to the same value.
        /// </summary>
        /// <param name="value">The x, y and z coordinates in 3d-space.</param>
        public Vector2i(int value)
        {
            X = value;
            Z = value;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y from <see cref="T:Microsoft.Xna.Framework.Vector3" />.
        /// </summary>
        /// <param name="value">The x, y and z coordinates in 3d-space.</param>
        public Vector2i(Vector3 value)
        {
            X = (int)value.X;
            Z = (int)value.Z;
        }

        /// <summary>
        /// Constructs a 3d vector with X, Y from <see cref="T:Microsoft.Xna.Framework.Vector2" /> and Z from a scalar.
        /// </summary>
        /// <param name="value">The x and y coordinates in 3d-space.</param>
        /// <param name="z">The z coordinate in 3d-space.</param>
        public Vector2i(Vector2 value)
        {
            X = (int)value.X;
            Z = (int)value.Y;
        }

        /// <summary>
        /// Performs vector addition on <paramref name="value1" /> and <paramref name="value2" />.
        /// </summary>
        /// <param name="value1">The first vector to add.</param>
        /// <param name="value2">The second vector to add.</param>
        /// <returns>The result of the vector addition.</returns>
        public static Vector2i Add(Vector2i value1, Vector2i value2)
        {
            value1.X += value2.X;
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
        public static void Add(ref Vector2i value1, ref Vector2i value2, out Vector2i result)
        {
            result.X = value1.X + value2.X;
            result.Z = value1.Z + value2.Z;
        }

        /// <summary>
        /// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector2i" /> towards positive infinity.
        /// </summary>
        public void Ceiling()
        {
            X = (int)Math.Ceiling((double)X);
            Z = (int)Math.Ceiling((double)Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains members from another vector rounded towards positive infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public static Vector2i Ceiling(Vector2i value)
        {
            value.X = (int)Math.Ceiling((double)value.X);
            value.Z = (int)Math.Ceiling((double)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains members from another vector rounded towards positive infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        public static void Ceiling(ref Vector2i value, out Vector2i result)
        {
            result.X = (int)Math.Ceiling((double)value.X);
            result.Z = (int)Math.Ceiling((double)value.Z);
        }

        /// <summary>Clamps the specified value within a range.</summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <returns>The clamped value.</returns>
        public static Vector2i Clamp(Vector2i value1, Vector2i min, Vector2i max)
        {
            return new Vector2i(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Z, min.Z, max.Z));
        }

        /// <summary>Clamps the specified value within a range.</summary>
        /// <param name="value1">The value to clamp.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="result">The clamped value as an output parameter.</param>
        public static void Clamp(
            ref Vector2i value1,
            ref Vector2i min,
            ref Vector2i max,
            out Vector2i result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
        }

        /// <summary>Returns the distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The distance between two vectors.</returns>
        public static int Distance(Vector2i value1, Vector2i value2)
        {
            int result;
            DistanceSquared(ref value1, ref value2, out result);
            return (int)Math.Sqrt((double)result);
        }

        /// <summary>Returns the distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The distance between two vectors as an output parameter.</param>
        public static void Distance(ref Vector2i value1, ref Vector2i value2, out int result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (int)Math.Sqrt((double)result);
        }

        /// <summary>Returns the squared distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between two vectors.</returns>
        public static int DistanceSquared(Vector2i value1, Vector2i value2)
        {
            return (int)(((double)value1.X - (double)value2.X) * ((double)value1.X - (double)value2.X) + ((double)value1.Z - (double)value2.Z) * ((double)value1.Z - (double)value2.Z));
        }

        /// <summary>Returns the squared distance between two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The squared distance between two vectors as an output parameter.</param>
        public static void DistanceSquared(ref Vector2i value1, ref Vector2i value2, out int result) => result = (int)(((double)value1.X - (double)value2.X) * ((double)value1.X - (double)value2.X) + ((double)value1.Z - (double)value2.Z) * ((double)value1.Z - (double)value2.Z));

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector2i Divide(Vector2i value1, Vector2i value2)
        {
            value1.X /= value2.X;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector2i Divide(Vector2i value1, int divider)
        {
            var num = 1f / divider;
            value1.X = (int)(value1.X * num);
            value1.Z = (int)(value1.Z * num);
            return value1;
        }
        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector2i Divide(Vector2i value1, float divider)
        {
            var num = 1f / divider;
            value1.X = (int)(value1.X * num);
            value1.Z = (int)(value1.Z * num);
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
        public static void Divide(ref Vector2i value1, int divider, out Vector2i result)
        {
            var num = 1f / divider;
            result.X = (int)(value1.X * num);
            result.Z = (int)(value1.Z * num);
        }
        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="divider">Divisor scalar.</param>
        /// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
        public static void Divide(ref Vector2i value1, float divider, out Vector2i result)
        {
            var num = 1f / divider;
            result.X =(int)(value1.X * num);
            result.Z =(int)(value1.Z * num);
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The result of dividing the vectors as an output parameter.</param>
        public static void Divide(ref Vector2i value1, ref Vector2i value2, out Vector2i result)
        {
            result.X = value1.X / value2.X;
            result.Z = value1.Z / value2.Z;
        }

        /// <summary>Returns a dot product of two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The dot product of two vectors.</returns>
        public static int Dot(Vector2i value1, Vector2i value2) => (int)((double)value1.X * (double)value2.X + (double)value1.Z * (double)value2.Z);

        /// <summary>Returns a dot product of two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The dot product of two vectors as an output parameter.</param>
        public static void Dot(ref Vector2i value1, ref Vector2i value2, out int result) => result = (int)((double)value1.X * (double)value2.X + (double)value1.Z * (double)value2.Z);

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj) => obj is Vector2i Vector2i && (double)X == (double)Vector2i.X && (double)Z == (double)Vector2i.Z;

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Vector2i" /> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector2i other) => (double)X == (double)other.X &&  (double)Z == (double)other.Z;

        /// <summary>
        /// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector2i" /> towards negative infinity.
        /// </summary>
        public void Floor()
        {
            X = (int)Math.Floor((double)X);
            Z = (int)Math.Floor((double)Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains members from another vector rounded towards negative infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public static Vector2i Floor(Vector2i value)
        {
            value.X = (int)Math.Floor((double)value.X);
            value.Z = (int)Math.Floor((double)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains members from another vector rounded towards negative infinity.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        public static void Floor(ref Vector2i value, out Vector2i result)
        {
            result.X = (int)Math.Floor((double)value.X);
            result.Z = (int)Math.Floor((double)value.Z);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public override int GetHashCode() => X.GetHashCode() * 397 ^ Z.GetHashCode();

        /// <summary>
        /// Returns the length of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <returns>The length of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public int Length() => (int)Math.Sqrt((double)X * (double)X + (double)Z * (double)Z);

        /// <summary>
        /// Returns the squared length of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <returns>The squared length of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public int LengthSquared() => (int)((double)X * (double)X + (double)Z * (double)Z);

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector2i Lerp(Vector2i value1, Vector2i value2, int amount) => new Vector2i(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Z, value2.Z, amount));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains linear interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void Lerp(
            ref Vector2i value1,
            ref Vector2i value2,
            int amount,
            out Vector2i result)
        {
            result.X = (int)MathHelper.Lerp(value1.X, value2.X, amount);
            result.Z = (int)MathHelper.Lerp(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector2i.Lerp(Microsoft.Xna.Framework.Vector2i,Microsoft.Xna.Framework.Vector2i,System.Single)" />.
        /// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <returns>The result of linear interpolation of the specified vectors.</returns>
        public static Vector2i LerpPrecise(Vector2i value1, Vector2i value2, int amount) => new Vector2i(MathHelper.LerpPrecise(value1.X, value2.X, amount), MathHelper.LerpPrecise(value1.Z, value2.Z, amount));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains linear interpolation of the specified vectors.
        /// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
        /// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector2i.Lerp(Microsoft.Xna.Framework.Vector2i@,Microsoft.Xna.Framework.Vector2i@,System.Single,Microsoft.Xna.Framework.Vector2i@)" />.
        /// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
        /// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
        public static void LerpPrecise(
            ref Vector2i value1,
            ref Vector2i value2,
            int amount,
            out Vector2i result)
        {
            result.X = (int)MathHelper.LerpPrecise(value1.X, value2.X, amount);
            result.Z = (int)MathHelper.LerpPrecise(value1.Z, value2.Z, amount);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with maximal values from the two vectors.</returns>
        public static Vector2i Max(Vector2i value1, Vector2i value2) => new Vector2i(MathHelper.Max(value1.X, value2.X), MathHelper.Max(value1.Z, value2.Z));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a maximal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with maximal values from the two vectors as an output parameter.</param>
        public static void Max(ref Vector2i value1, ref Vector2i value2, out Vector2i result)
        {
            result.X = MathHelper.Max(value1.X, value2.X);
            result.Z = MathHelper.Max(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with minimal values from the two vectors.</returns>
        public static Vector2i Min(Vector2i value1, Vector2i value2) => new Vector2i(MathHelper.Min(value1.X, value2.X), MathHelper.Min(value1.Z, value2.Z));

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a minimal values from the two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector2i" /> with minimal values from the two vectors as an output parameter.</param>
        public static void Min(ref Vector2i value1, ref Vector2i value2, out Vector2i result)
        {
            result.X = MathHelper.Min(value1.X, value2.X);
            result.Z = MathHelper.Min(value1.Z, value2.Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The result of the vector multiplication.</returns>
        public static Vector2i Multiply(Vector2i value1, Vector2i value2)
        {
            value1.X *= value2.X;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector2i" /> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <returns>The result of the vector multiplication with a scalar.</returns>
        public static Vector2i Multiply(Vector2i value1, int scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector2i" /> and a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="scaleFactor">Scalar value.</param>
        /// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
        public static void Multiply(ref Vector2i value1, int scaleFactor, out Vector2i result)
        {
            result.X = value1.X * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a multiplication of two vectors.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The result of the vector multiplication as an output parameter.</param>
        public static void Multiply(ref Vector2i value1, ref Vector2i value2, out Vector2i result)
        {
            result.X = value1.X * value2.X;
            result.Z = value1.Z * value2.Z;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The result of the vector inversion.</returns>
        public static Vector2i Negate(Vector2i value)
        {
            value = new Vector2i(-value.X, -value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains the specified vector inversion.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The result of the vector inversion as an output parameter.</param>
        public static void Negate(ref Vector2i value, out Vector2i result)
        {
            result.X = -value.X;
            result.Z = -value.Z;
        }

        /// <summary>
        /// Turns this <see cref="T:Microsoft.Xna.Framework.Vector2i" /> to a unit vector with the same direction.
        /// </summary>
        public void Normalize()
        {
            var num = 1f / (int)Math.Sqrt((double)X * (double)X + (double)Z * (double)Z);
            X =(int)(X * num);
            Z =(int)(Z * num);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a normalized values from another vector.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>Unit vector.</returns>
        public static Vector2i Normalize(Vector2i value)
        {
            var num = 1f / (int)Math.Sqrt((double)value.X * (double)value.X + (double)value.Z * (double)value.Z);
            return new Vector2i(value.X * num, value.Z * num);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains a normalized values from another vector.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">Unit vector as an output parameter.</param>
        public static void Normalize(ref Vector2i value, out Vector2i result)
        {
            var num = 1f / (int)Math.Sqrt((double)value.X * (double)value.X + (double)value.Z * (double)value.Z);
            result.X = (int)(value.X * num);
            result.Z = (int)(value.Z * num);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <returns>Reflected vector.</returns>
        public static Vector2i Reflect(Vector2i vector, Vector2i normal)
        {
            var num = (int)((double)vector.X * (double)normal.X + (double)vector.Z * (double)normal.Z);
            Vector2i Vector2i;
            Vector2i.X = (int)(vector.X - 2f * normal.X * num);
            Vector2i.Z = (int)(vector.Z - 2f * normal.Z * num);
            return Vector2i;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains reflect vector of the given vector and normal.
        /// </summary>
        /// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="normal">Reflection normal.</param>
        /// <param name="result">Reflected vector as an output parameter.</param>
        public static void Reflect(ref Vector2i vector, ref Vector2i normal, out Vector2i result)
        {
            var num = (int)((double)vector.X * (double)normal.X + (double)vector.Z * (double)normal.Z);
            result.X = (int)(vector.X - 2f * normal.X * num);
            result.Z = (int)(vector.Z - 2f * normal.Z * num);
        }

        /// <summary>
        /// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector2i" /> towards the nearest integer value.
        /// </summary>
        public void Round()
        {
            X = (int)Math.Round((double)X);
            Z = (int)Math.Round((double)Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains members from another vector rounded to the nearest integer value.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public static Vector2i Round(Vector2i value)
        {
            value.X = (int)Math.Round((double)value.X);
            value.Z = (int)Math.Round((double)value.Z);
            return value;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains members from another vector rounded to the nearest integer value.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        public static void Round(ref Vector2i value, out Vector2i result)
        {
            result.X = (int)Math.Round((double)value.X);
            result.Z = (int)Math.Round((double)value.Z);
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector2i" /> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <returns>The result of the vector subtraction.</returns>
        public static Vector2i Subtract(Vector2i value1, Vector2i value2)
        {
            value1.X -= value2.X;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2i" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector2i" /> from a another.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</param>
        /// <param name="result">The result of the vector subtraction as an output parameter.</param>
        public static void Subtract(ref Vector2i value1, ref Vector2i value2, out Vector2i result)
        {
            result.X = value1.X - value2.X;
            result.Z = value1.Z - value2.Z;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector2i" /> in the format:
        /// {X:[<see cref="F:Microsoft.Xna.Framework.Vector2i.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Vector2i.Y" />] Z:[<see cref="F:Microsoft.Xna.Framework.Vector2i.Z" />]}
        /// </summary>
        /// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector2i" />.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder(32);
            stringBuilder.Append("{X:");
            stringBuilder.Append(X);
            stringBuilder.Append(" Z:");
            stringBuilder.Append(Z);
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Deconstruct(out int x, out int z)
        {
            x = X;
            z = Z;
        }

        /// <summary>
        /// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector2i" /> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector2i" /> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector2i" /> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Vector2i value1, Vector2i value2) => (double)value1.X == (double)value2.X && (double)value1.Z == (double)value2.Z;

        /// <summary>
        /// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector2i" /> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector2i" /> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector2i" /> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Vector2i value1, Vector2i value2) => !(value1 == value2);

        /// <summary>Adds two vectors.</summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the right of the add sign.</param>
        /// <returns>Sum of the vectors.</returns>
        public static Vector2i operator +(Vector2i value1, Vector2i value2)
        {
            value1.X += value2.X;
            value1.Z += value2.Z;
            return value1;
        }

        /// <summary>
        /// Inverts values in the specified <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the right of the sub sign.</param>
        /// <returns>Result of the inversion.</returns>
        public static Vector2i operator -(Vector2i value)
        {
            value = new Vector2i(-value.X, -value.Z);
            return value;
        }

        /// <summary>
        /// Subtracts a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> from a <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the right of the sub sign.</param>
        /// <returns>Result of the vector subtraction.</returns>
        public static Vector2i operator -(Vector2i value1, Vector2i value2)
        {
            value1.X -= value2.X;
            value1.Z -= value2.Z;
            return value1;
        }

        /// <summary>
        /// Multiplies the components of two vectors by each other.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the left of the mul sign.</param>
        /// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication.</returns>
        public static Vector2i operator *(Vector2i value1, Vector2i value2)
        {
            value1.X *= value2.X;
            value1.Z *= value2.Z;
            return value1;
        }

        /// <summary>Multiplies the components of vector by a scalar.</summary>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the left of the mul sign.</param>
        /// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector2i operator *(Vector2i value, float scaleFactor)
        {
            value.X =(int)(value.X* scaleFactor);
            value.Z =(int)(value.Z* scaleFactor);
            return value;
        }

        /// <summary>Multiplies the components of vector by a scalar.</summary>
        /// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
        /// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the right of the mul sign.</param>
        /// <returns>Result of the vector multiplication with a scalar.</returns>
        public static Vector2i operator *(int scaleFactor, Vector2i value)
        {
            value.X *= scaleFactor;
            value.Z *= scaleFactor;
            return value;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector2i" />.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the left of the div sign.</param>
        /// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the right of the div sign.</param>
        /// <returns>The result of dividing the vectors.</returns>
        public static Vector2i operator /(Vector2i value1, Vector2i value2)
        {
            value1.X /= value2.X;
            value1.Z /= value2.Z;
            return value1;
        }

        /// <summary>
        /// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2i" /> by a scalar.
        /// </summary>
        /// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2i" /> on the left of the div sign.</param>
        /// <param name="divider">Divisor scalar on the right of the div sign.</param>
        /// <returns>The result of dividing a vector by a scalar.</returns>
        public static Vector2i operator /(Vector2i value1, float divider)
        {
            var num = 1f / divider;
            value1.X =(int)(value1.X* num);
            value1.Z =(int)(value1.Z* num);
            return value1;
        }
        
        public static implicit operator Vector3 (Vector2i v)
        {
            return new Vector3(v.X, 0, v.Z);
        }        
        public static explicit operator Vector2i (Vector3 v)
        {
            return new Vector2i(v.X, v.Z);
        }
        
    }
}