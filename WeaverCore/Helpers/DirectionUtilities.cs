using UnityEngine;

namespace WeaverCore.Helpers
{
	public static class DirectionUtilities
	{
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

		public static float ToRads(this CardinalDirection direction)
		{
			return direction.ToDegrees() * Mathf.Deg2Rad;
		}

		public static CardinalDirection RadToDirection(float rads)
		{
			return DegreesToDirection(rads * Mathf.Rad2Deg);
		}

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
