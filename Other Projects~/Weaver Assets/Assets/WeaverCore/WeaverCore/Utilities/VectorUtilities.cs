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

		public static Vector2 DegreesToVector(this float angle)
		{
			return RadiansToVector(angle * Mathf.Deg2Rad);
		}

		public static Vector2 RadiansToVector(this float angle)
		{
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		}

		public static Vector3 AngleToVector(float radiansAngle, float magnitude = 1f)
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
	}
}
