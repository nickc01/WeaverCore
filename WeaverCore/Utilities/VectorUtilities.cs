using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to vectors
    /// </summary>
    public static class VectorUtilities
	{
        /// <summary>
        /// Creates a new <see cref="Color"/> based on the input color, with optional modifications to specific color components.
        /// </summary>
        /// <param name="c">The input <see cref="Color"/> to start with.</param>
        /// <param name="r">The new value for the red component. If not specified (or set to <see cref="float.NaN"/>), the red component remains unchanged.</param>
        /// <param name="g">The new value for the green component. If not specified (or set to <see cref="float.NaN"/>), the green component remains unchanged.</param>
        /// <param name="b">The new value for the blue component. If not specified (or set to <see cref="float.NaN"/>), the blue component remains unchanged.</param>
        /// <param name="a">The new value for the alpha (transparency) component. If not specified (or set to <see cref="float.NaN"/>), the alpha component remains unchanged.</param>
        /// <returns>A new <see cref="Color"/> based on the input color with optional modifications.</returns>
        public static Color With(this Color c, float r = float.NaN, float g = float.NaN, float b = float.NaN, float a = float.NaN)
        {
            if (!float.IsNaN(r))
            {
                c.r = r;
            }
            if (!float.IsNaN(g))
            {
                c.g = g;
            }
            if (!float.IsNaN(b))
            {
                c.b = b;
            }
            if (!float.IsNaN(a))
            {
                c.a = a;
            }
            return c;
        }


        /// <summary>
        /// Takes an existing vector, and a returns a new one with some fields modified
        /// </summary>
        /// <param name="v">The vector to base the new one from</param>
        /// <param name="x">The new x-value. If left at NaN, the field will be left unchanged</param>
        /// <param name="y">The new y-value. If left at NaN, the field will be left unchanged</param>
        /// <param name="z">The new z-value. If left at NaN, the field will be left unchanged</param>
        /// <param name="w">The new w-value. If left at NaN, the field will be left unchanged</param>
        /// <returns>Returns a new vector with the modified fields</returns>
        public static Vector4 With(this Vector4 v, float x = float.NaN, float y = float.NaN, float z = float.NaN, float w = float.NaN)
		{
			if (!float.IsNaN(x))
			{
				v.x = x;
			}
			if (!float.IsNaN(y))
			{
				v.y = y;
			}
			if (!float.IsNaN(z))
			{
				v.z = z;
			}
			if (!float.IsNaN(w))
			{
				v.w = w;
			}
			return v;
		}

		/// <summary>
		/// Takes an existing vector, and a returns a new one with some fields modified
		/// </summary>
		/// <param name="v">The vector to base the new one from</param>
		/// <param name="x">The new x-value. If left at NaN, the field will be left unchanged</param>
		/// <param name="y">The new y-value. If left at NaN, the field will be left unchanged</param>
		/// <param name="z">The new z-value. If left at NaN, the field will be left unchanged</param>
		/// <returns>Returns a new vector with the modified fields</returns>
		public static Vector3 With(this Vector3 v, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			if (!float.IsNaN(x))
			{
				v.x = x;
			}
			if (!float.IsNaN(y))
			{
				v.y = y;
			}
			if (!float.IsNaN(z))
			{
				v.z = z;
			}
			return v;
		}

		/// <summary>
		/// Takes an existing vector, and a returns a new one with some fields modified
		/// </summary>
		/// <param name="v">The vector to base the new one from</param>
		/// <param name="x">The new x-value. If left at NaN, the field will be left unchanged</param>
		/// <param name="y">The new y-value. If left at NaN, the field will be left unchanged</param>
		/// <returns>Returns a new vector with the modified fields</returns>
		public static Vector2 With(this Vector2 v, float x = float.NaN, float y = float.NaN)
		{
			if (!float.IsNaN(x))
			{
				v.x = x;
			}
			if (!float.IsNaN(y))
			{
				v.y = y;
			}
			return v;
		}

        /// <summary>
        /// Flips the W component of a <see cref="Vector4"/> while keeping the X, Y, and Z components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector4"/>.</param>
        /// <returns>A new <see cref="Vector4"/> with the W component flipped.</returns>
        public static Vector4 FlipW(this Vector4 v) => new Vector4(v.x, v.y, v.z, -v.w);

        /// <summary>
        /// Flips the Z component of a <see cref="Vector4"/> while keeping the X, Y, and W components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector4"/>.</param>
        /// <returns>A new <see cref="Vector4"/> with the Z component flipped.</returns>
        public static Vector4 FlipZ(this Vector4 v) => new Vector4(v.x, v.y, -v.z, v.w);

        /// <summary>
        /// Flips the Y component of a <see cref="Vector4"/> while keeping the X, Z, and W components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector4"/>.</param>
        /// <returns>A new <see cref="Vector4"/> with the Y component flipped.</returns>
        public static Vector4 FlipY(this Vector4 v) => new Vector4(v.x, -v.y, v.z, v.w);

        /// <summary>
        /// Flips the X component of a <see cref="Vector4"/> while keeping the Y, Z, and W components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector4"/>.</param>
        /// <returns>A new <see cref="Vector4"/> with the X component flipped.</returns>
        public static Vector4 FlipX(this Vector4 v) => new Vector4(-v.x, v.y, v.z, v.w);

        /// <summary>
        /// Flips the Z component of a <see cref="Vector3"/> while keeping the X and Y components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector3"/>.</param>
        /// <returns>A new <see cref="Vector3"/> with the Z component flipped.</returns>
        public static Vector3 FlipZ(this Vector3 v) => new Vector3(v.x, v.y, -v.z);

        /// <summary>
        /// Flips the Y component of a <see cref="Vector3"/> while keeping the X and Z components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector3"/>.</param>
        /// <returns>A new <see cref="Vector3"/> with the Y component flipped.</returns>
        public static Vector3 FlipY(this Vector3 v) => new Vector3(v.x, -v.y, v.z);

        /// <summary>
        /// Flips the X component of a <see cref="Vector3"/> while keeping the Y and Z components unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector3"/>.</param>
        /// <returns>A new <see cref="Vector3"/> with the X component flipped.</returns>
        public static Vector3 FlipX(this Vector3 v) => new Vector3(-v.x, v.y, v.z);

        /// <summary>
        /// Flips the Y component of a <see cref="Vector2"/> while keeping the X component unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector2"/>.</param>
        /// <returns>A new <see cref="Vector2"/> with the Y component flipped.</returns>
        public static Vector2 FlipY(this Vector2 v) => new Vector2(v.x, -v.y);

        /// <summary>
        /// Flips the X component of a <see cref="Vector2"/> while keeping the Y component unchanged.
        /// </summary>
        /// <param name="v">The input <see cref="Vector2"/>.</param>
        /// <returns>A new <see cref="Vector2"/> with the X component flipped.</returns>
        public static Vector2 FlipX(this Vector2 v) => new Vector2(-v.x, v.y);


        /// <summary>
        /// Converts an angle (in degrees) into a vector pointing in the same direction
        /// </summary>
        /// <param name="angle">The angle to convert to a vector</param>
        /// <param name="magnitude">The magnitude of the final vector</param>
        /// <returns>The vector that points in the direction of the angle</returns>
        public static Vector2 DegreesToVector(this float angle, float magnitude = 1f)
		{
			return RadiansToVector(angle * Mathf.Deg2Rad, magnitude);
		}

		/// <summary>
		/// Converts a vector into an angle (in degrees)
		/// </summary>
		/// <param name="vector">The vector to convert</param>
		/// <returns>Returns an angle that points in the direction of the vector</returns>
		public static float VectorToDegrees(this Vector2 vector)
		{
			return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Converts a vector into an angle (in radians)
		/// </summary>
		/// <param name="vector">The vector to convert</param>
		/// <returns>Returns an angle that points in the direction of the vector</returns>
		public static float VectorToRadians(this Vector2 vector)
		{
			return Mathf.Atan2(vector.y, vector.x);
		}

		/// <summary>
		/// Converts a vector into an angle (in degrees)
		/// </summary>
		/// <param name="vector">The vector to convert</param>
		/// <returns>Returns an angle that points in the direction of the vector</returns>
		public static float VectorToDegrees(this Vector3 vector)
		{
			return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Converts a vector into an angle (in radians)
		/// </summary>
		/// <param name="vector">The vector to convert</param>
		/// <returns>Returns an angle that points in the direction of the vector</returns>
		public static float VectorToRadians(this Vector3 vector)
		{
			return Mathf.Atan2(vector.y, vector.x);
		}

		/// <summary>
		/// Converts an angle (in radians) into a vector
		/// </summary>
		/// <param name="radiansAngle">The angle to convert into a vector</param>
		/// <param name="magnitude">The magnitude of the final vector</param>
		/// <returns>Returns a vector that points in the direction of the angle</returns>
		public static Vector3 RadiansToVector(float radiansAngle, float magnitude = 1f)
		{
			return new Vector3(Mathf.Cos(radiansAngle) * magnitude, Mathf.Sin(radiansAngle) * magnitude);
		}

		/// <summary>
		/// Gets the angle (in degrees) between two vectors
		/// </summary>
		/// <param name="Source">The first angle</param>
		/// <param name="Destination">The second angle</param>
		/// <returns>Returns the angle between the two vectors</returns>
		public static float GetAngleBetween(this Vector3 Source, Vector3 Destination)
		{
			return Mathf.Atan2(Destination.y - Source.y, Destination.x - Source.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// Multiplies the components of the source vector with the components of the deceleration vector. If any of the deceneration components are float.NaN, that component will be ignored. If the x component of deceleration is float.NaN, the x component of source will not be changed
		/// </summary>
		/// <param name="source">The source vector to decelerate</param>
		/// <param name="deceleration">The amount of deceleration to be applied to the source vector</param>
		/// <returns>The decelerated source vector</returns>
		public static Vector2 Decelerate(Vector2 source, Vector2 deceleration)
		{
			if (!float.IsNaN(deceleration.x))
			{
				source.x *= deceleration.x;
			}
			if (!float.IsNaN(deceleration.y))
			{
				source.y *= deceleration.y;
			}
			return source;
		}

		/// <summary>
		/// Multiplies the components of the source vector with the components of the deceleration vector. If any of the deceneration components are float.NaN, that component will be ignored. If the x component of deceleration is float.NaN, the x component of source will not be changed
		/// </summary>
		/// <param name="source">The source vector to decelerate</param>
		/// <param name="deceleration">The amount of deceleration to be applied to the source vector</param>
		/// <returns>The decelerated source vector</returns>
		public static Vector3 Decelerate(Vector3 source, Vector3 deceleration)
		{
			if (!float.IsNaN(deceleration.x))
			{
				source.x *= deceleration.x;
			}
			if (!float.IsNaN(deceleration.y))
			{
				source.y *= deceleration.y;
			}
			if (!float.IsNaN(deceleration.z))
			{
				source.z *= deceleration.z;
			}
			return source;
		}

		/// <summary>
		/// Creates an array of values that are evenly spaced together.
		/// </summary>
		/// <param name="amountOfValues">The amount of values to create</param>
		/// <param name="spacingBetweenValues">The amount of spacing between values</param>
		/// <returns>Returns a list of values that are evenly spaced between one another</returns>
		/// <example>
		/// For example, if <paramref name="amountOfValues"/> = 7, and <paramref name="spacingBetweenValues"/> = 6, then the following list will be created:
		/// [0, -6, 6, -12, 12, -18, 18]
		/// 
		/// If <paramref name="amountOfValues"/> = 8, and <paramref name="spacingBetweenValues"/> = 4, then the following list will be created:
		/// [-2, 2, -6, 6, -10, 10, -14, 14]
		/// </example>
		public static float[] CalculateSpacedValues(int amountOfValues, float spacingBetweenValues)
		{
			float[] angles = new float[amountOfValues];
			if (amountOfValues == 0)
			{
				return angles;
			}
			amountOfValues = Mathf.Abs(amountOfValues);
			spacingBetweenValues = Mathf.Abs(spacingBetweenValues);

			if (amountOfValues % 2 == 1)
			{
				angles[0] = 0f;
				amountOfValues--;

				if (amountOfValues > 0)
				{
					for (int i = 0; i < amountOfValues / 2; i++)
					{
						angles[1 + (i * 2)] = spacingBetweenValues * (i + 1);
						angles[2 + (i * 2)] = -spacingBetweenValues * (i + 1);
					}
				}
			}
			else
			{
				float startLeft = spacingBetweenValues / 2;
				float startRight = -startLeft;
				angles[0] = startLeft;
				angles[1] = startRight;

				amountOfValues -= 2;

				if (amountOfValues > 0)
				{
					for (int i = 0; i < amountOfValues / 2; i++)
					{
						angles[2 + (i * 2)] = startLeft + (spacingBetweenValues * (i + 1));
						angles[3 + (i * 2)] = startRight - (spacingBetweenValues * (i + 1));
					}
				}
			}

			return angles;
		}

		public static float RandomInRange(this Vector2 range)
        {
			return UnityEngine.Random.Range(range.x, range.y);
        }

		public static float ClampInRange(this Vector2 range, float value)
		{
			return Mathf.Clamp(value, range.x, range.y);
		}


        public static int RandomInRange(this Vector2Int range)
        {
            return UnityEngine.Random.Range(range.x, range.y + 1);
        }

        public static int ClampInRange(this Vector2Int range, int value)
        {
			if (value < range.x)
			{
				value = range.x;
			}

			if (value > range.y)
			{
				value = range.y;
			}

			return value;
        }

		public static bool IsWithinRange(this Vector2 range, float v)
		{
			return v >= range.x && v <= range.y;
		}

		public static Vector3 Multiply(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static System.Numerics.Vector3 ToV3(this Vector3 v)
		{
			return new System.Numerics.Vector3(v.x, v.y, v.z);
		}

        public static Vector3 ToVector3(this System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
