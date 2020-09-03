using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	public static class VectorUtilities
	{
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

		public static Vector2 DegreesToVector(this float angle, float magnitude = 1f)
		{
			return RadiansToVector(angle * Mathf.Deg2Rad, magnitude);
		}

		public static float VectorToDegrees(this Vector2 vector)
		{
			return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
		}

		public static float VectorToRadians(this Vector2 vector)
		{
			return Mathf.Atan2(vector.y, vector.x);
		}

		public static float VectorToDegrees(this Vector3 vector)
		{
			return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
		}

		public static float VectorToRadians(this Vector3 vector)
		{
			return Mathf.Atan2(vector.y, vector.x);
		}

		/*public static Vector2 RadiansToVector(this float angle)
		{
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		}*/

		public static Vector3 RadiansToVector(float radiansAngle, float magnitude = 1f)
		{
			return new Vector3(Mathf.Cos(radiansAngle) * magnitude, Mathf.Sin(radiansAngle) * magnitude);
		}

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

		/*public static void DecomposeVector(this Vector2 vector, out float angle, out float magnitude)
		{
			angle = VectorToDegrees(vector);
			magnitude = vector.magnitude;
		}*/

		public static List<float> CalculateSpacedValues(int amountOfValues, float amountBetweenValues)
		{
			List<float> angles = new List<float>();
			if (amountOfValues == 0)
			{
				return angles;
			}
			amountOfValues = Mathf.Abs(amountOfValues);
			amountBetweenValues = Mathf.Abs(amountBetweenValues);

			if (amountOfValues % 2 == 1)
			{
				angles.Add(0);
				amountOfValues--;

				if (amountOfValues > 0)
				{
					for (int i = 0; i < amountOfValues / 2; i++)
					{
						//yield return angleBetweenObjects * (i + 1);
						//yield return -angleBetweenObjects * (i + 1);
						angles.Add(amountBetweenValues * (i + 1));
						angles.Add(-amountBetweenValues * (i + 1));
					}
				}
			}
			else
			{
				float startLeft = amountBetweenValues / 2;
				float startRight = -startLeft;

				//yield return startLeft;
				//yield return startRight;
				angles.Add(startLeft);
				angles.Add(startRight);

				amountOfValues -= 2;

				if (amountOfValues > 0)
				{
					for (int i = 0; i < amountOfValues / 2; i++)
					{
						//yield return startLeft + (angleBetweenObjects * (i + 1));
						//yield return startRight - (angleBetweenObjects * (i + 1));
						angles.Add(startLeft + (amountBetweenValues * (i + 1)));
						angles.Add(startRight - (amountBetweenValues * (i + 1)));
					}
				}
			}

			return angles;
		}
	}
}
