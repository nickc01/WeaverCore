using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Utilities
{

    /// <summary>
    /// Contains many utility functions for handling cardinal directions
    /// </summary>
    public static class DirectionUtilities
	{
		/// <summary>
		/// Converts a cardinal direction to degrees
		/// </summary>
		/// <param name="direction">The input cardinal direction</param>
		/// <returns>The angle in degrees</returns>
		public static float ToDegrees(this CardinalDirection direction)
		{
			switch (direction)
			{
				case CardinalDirection.Up:
					return 90f;
				case CardinalDirection.Down:
					return 270f;
				case CardinalDirection.Left:
					return 180f;
				case CardinalDirection.Right:
					return 0f;
				default:
					return 0f;
			}
		}

		/// <summary>
		/// Converts the cardinal direction to radians
		/// </summary>
		/// <param name="direction">The input cardinal direction</param>
		/// <returns>The angle in radians</returns>
		public static float ToRads(this CardinalDirection direction)
		{
			return direction.ToDegrees() * Mathf.Deg2Rad;
		}

		/// <summary>
		/// Converts a radian angle to the closest cardinal direction it matches with
		/// </summary>
		/// <param name="rads">The angle in radians</param>
		/// <returns>The approximate cardinal direction of the angle</returns>
		public static CardinalDirection RadToDirection(float rads)
		{
			return DegreesToDirection(rads * Mathf.Rad2Deg);
		}

		/// <summary>
		/// Converts a degrees angle to the closest cardinal direction it matches with
		/// </summary>
		/// <param name="degrees">The angle in degrees</param>
		/// <returns>The approximate cardinal direction of the angle</returns>
		public static CardinalDirection DegreesToDirection(float degrees)
		{
			degrees %= 360;
			if (degrees < 0)
			{
				degrees += 360;
			}

			if (degrees > 45 && degrees <= 135)
			{
				return CardinalDirection.Up;
			}
			else if (degrees > 135 && degrees <= 225)
			{
				return CardinalDirection.Left;
			}
			else if (degrees > 225 && degrees <= 315)
			{
				return CardinalDirection.Down;
			}
			else
			{
				return CardinalDirection.Right;
			}
		}
	}
}
