using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class RaycastUtilities
    {
        static RaycastHit2D[] hitCache = new RaycastHit2D[1];
        static int terrainMask = -1;

        public static int TerrainMask => terrainMask == -1 ? terrainMask = LayerMask.GetMask("Terrain") : terrainMask;

        public static bool SimpleCastTo(Vector2 origin, Vector2 dest, out RaycastHit2D result, float distance = float.NaN, int layerMask = -1)
        {
            if (float.IsNaN(distance))
            {
                distance = (dest - origin).magnitude;
            }

            if (distance < 0f)
            {
                distance = (dest - origin).magnitude - distance;
            }

            return SimpleCast(origin, (dest - origin).normalized, out result, distance, layerMask);
        }

        public static bool SimpleCast(Vector2 origin, Vector2 direction, out RaycastHit2D result, float distance, int layerMask = -1)
        {
            if (layerMask < 0)
            {
                layerMask = TerrainMask;
            }
            var count = Physics2D.RaycastNonAlloc(origin, direction, hitCache, distance, layerMask);

            if (count > 0)
            {
                result = hitCache[0];
            }
            else
            {
                result = default;
            }

            return count > 0;
        }

        static int TryRaycast(Vector2 center, Vector2 direction, RaycastHit2D[] hitCache, float distance, int terrainMask)
		{
			var result = Physics2D.RaycastNonAlloc(center, direction, hitCache, distance, terrainMask);

			if (result > 0)
			{
				Debug.DrawLine(center, hitCache[0].point, Color.magenta, 1f);
			}
			else
			{
				Debug.DrawLine(center, center + (direction * distance), Color.magenta, 1f);
			}

			return result;
		}

		public static Vector2 FindMaxInDirection(Vector2 center, float targetX, float raycastDistance = 3f)
		{
			const float X_PRECISION = 0.5f;
			const float Y_PRECISION = 0.01f;

			Vector2 centerPoint;
			if (TryRaycast(center, Vector2.down, hitCache, raycastDistance, terrainMask) > 0)
			{
				centerPoint = hitCache[0].point;
			}
			else
			{
				centerPoint = (Vector2)center + (Vector2.down * raycastDistance);
			}

			Vector2 farthestPoint = centerPoint;

			if (targetX > center.x)
			{
				for (float i = centerPoint.x; i <= targetX; i += X_PRECISION)
				{
					var start = new Vector2(i, center.y);
					Vector2 point;
					if (TryRaycast(start, Vector2.down, hitCache, raycastDistance, terrainMask) > 0)
					{
						point = hitCache[0].point;
					}
					else
					{
						point = start + (Vector2.down * raycastDistance);
					}

					if (Mathf.Abs(point.y - centerPoint.y) <= Y_PRECISION)
					{
						farthestPoint = point;
					}
					else
					{
						break;
					}
				}
			}
			else
			{
				for (float i = centerPoint.x; i >= targetX; i -= X_PRECISION)
				{
					var start = new Vector2(i, center.y);
					Vector2 point;
					if (TryRaycast(start, Vector2.down, hitCache, raycastDistance, terrainMask) > 0)
					{
						point = hitCache[0].point;
					}
					else
					{
						point = start + (Vector2.down * raycastDistance);
					}

					if (Mathf.Abs(point.y - centerPoint.y) <= Y_PRECISION)
					{
						farthestPoint = point;
					}
					else
					{
						break;
					}
				}
			}

			Debug.DrawLine(center, farthestPoint, Color.cyan, 1f);

			return farthestPoint;
		}
    }
}