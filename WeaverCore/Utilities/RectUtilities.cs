using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class RectUtilities
	{
		public static Vector2 ClampWithin(this Rect rect, Vector2 point)
		{
			return new Vector2(Mathf.Clamp(point.x, rect.xMin, rect.xMax), Mathf.Clamp(point.y, rect.yMin, rect.yMax));
		}
	}
}
