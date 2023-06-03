using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains some utility functions related to math
	/// </summary>
	public static class MathUtilities
	{
		/// <summary>
		/// Calculates the 2D velocity needed to reach the specified <paramref name="end"/> point.
		/// </summary>
		/// <param name="start">The starting position</param>
		/// <param name="end">The end position</param>
		/// <param name="time">The amount of time needed to get from the <paramref name="start"/> to the <paramref name="end"/></param>
		/// <param name="gravityScale">The gravity multiplier</param>
		/// <returns>Returns the 2D velocity needed to reach the specified <paramref name="end"/> point.</returns>
		public static Vector2 CalculateVelocityToReachPoint(Vector2 start, Vector2 end, double time, double gravityScale = 1.0)
		{
			double a = Physics2D.gravity.y * gravityScale;

			double yDest = end.y - start.y;

			double yVelocity = (yDest / time) - (0.5 * a * time);
			double xVelocity = (end.x - (double)start.x) / time;

			return new Vector2((float)xVelocity, (float)yVelocity);
		}

		/// <summary>
		/// Calculates the vertical velocity needed to reach the height of <paramref name="endY"/>
		/// </summary>
		/// <param name="startY">The starting height</param>
		/// <param name="endY">The destination height</param>
		/// <param name="time">The amount of time needed to get from the <paramref name="startY"/> to the <paramref name="endY"/></param>
		/// <param name="gravityScale">The gravity multiplier</param>
		/// <returns>Returns the vertical velocity needed to reach the height of <paramref name="endY"/></returns>
		static float CalculateVerticalVelocity(float startY, float endY, float time, float gravityScale = 1f)
		{
			float a = Physics2D.gravity.y * gravityScale;
			float newY = endY - startY;

			return (newY / time) - (a * time);
		}

		/// <summary>
		/// This function will calculate a velocity curve needed to get from startY to endY, and will return the time it takes to reach the peak of the curve and the peak height of the curve itself
		/// </summary>
		/// <param name="startY">The starting height</param>
		/// <param name="endY">The destination height</param>
		/// <param name="time">The amount of time needed to get from <paramref name="startY"/> to <paramref name="endY"/></param>
		/// <param name="gravityScale">The gravity multiplier</param>
		/// <returns>the time it takes to reach the peak of the curve and the peak height of the curve itself</returns>
		public static (float timeToPeak, float peakHeight) CalculateMaximumOfCurve(float startY, float endY, float time, float gravityScale = 1f)
		{
			var velocity = CalculateVerticalVelocity(startY, endY, time, gravityScale);

			var a = gravityScale * Physics2D.gravity.y;

			var timeToPeak = -velocity / (2f * a);

			var peakValue = (a * timeToPeak * timeToPeak) + (velocity * timeToPeak);

			return (timeToPeak, peakValue);
		}

		/// <summary>
		/// Converts a set of polar coordinates to cartesian coordinates
		/// </summary>
		/// <param name="angleDegrees">The angle of the polar coordinate</param>
		/// <param name="magnitude">The length of the polar coordinate</param>
		/// <returns>Returns the set of polar coordinates in cartesian coordinates</returns>
		public static Vector2 PolarToCartesian(float angleDegrees,float magnitude)
		{
			return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angleDegrees) * magnitude,Mathf.Sin(Mathf.Deg2Rad * angleDegrees) * magnitude);
		}

		/// <summary>
		/// Converts a set of polar coordinates to cartesian coordinates
		/// </summary>
		/// <param name="angleAndMagnitude">The polar coordinates (x is the angle, y is the magnitude)</param>
		/// <returns>Returns the set of polar coordinates in cartesian coordinates</returns>
		public static Vector2 PolarToCartesian(Vector2 angleAndMagnitude)
		{
			return PolarToCartesian(angleAndMagnitude.x, angleAndMagnitude.y);
		}

        /// <summary>
        /// Converts a set of cartesian coordinates to polar coordinates
        /// </summary>
        /// <param name="vector">The x-y coordinates</param>
        /// <returns>Returns the polar coordinates (x is the angle in degrees, y is the magnitude)</returns>
        public static Vector2 CartesianToPolar(Vector2 vector)
		{
			return CartesianToPolar(vector.x, vector.y);
		}

		/// <summary>
		/// Converts a set of cartesian coordinates to polar coordinates
		/// </summary>
		/// <param name="x">The x coordinates</param>
		/// <param name="y">The y coordinates</param>
		/// <returns>Returns the polar coordinates (x is the angle in degrees, y is the magnitude)</returns>
		public static Vector2 CartesianToPolar(float x, float y)
		{
			return new Vector2(Mathf.Atan2(y,x) * Mathf.Rad2Deg,Mathf.Sqrt((x*x) + (y*y)));
		}

		/// <summary>
		/// Clamps an angle into the range -180 to 180
		/// </summary>
		/// <param name="degrees">The angle to clamp</param>
		public static float ClampRotation(float degrees)
        {
			degrees %= 360f;

            if (degrees > 180f)
            {
				degrees -= 360f;
            }
			return degrees;
        }

		public static bool AngleIsWithinRange(float angleDegrees, Vector2 range)
        {

			//Clamp to range 0 - 360
			angleDegrees = (360 + (angleDegrees % 360)) % 360;
			range.x = (3600000 + range.x) % 360;
			range.y = (3600000 + range.y) % 360;

			if (range.x < range.y)
            {
				return range.x <= angleDegrees && angleDegrees <= range.y;
            }
			return range.x <= angleDegrees && angleDegrees <= range.y;
		}
	}
}
