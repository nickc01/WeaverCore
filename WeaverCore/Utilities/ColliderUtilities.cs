using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class ColliderUtilities
    {
        public static Bounds GetBoundsSafe(this PolygonCollider2D collider2D)
        {
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            var points = collider2D.points;

            for (int i = 0; i < points.Length; i++)
            {
                var worldPoint = collider2D.transform.TransformPoint(points[i]);

                if (worldPoint.x < min.x)
                {
                    min.x = worldPoint.x;
                }

                if (worldPoint.y < min.y)
                {
                    min.y = worldPoint.y;
                }

                if (worldPoint.z < min.z)
                {
                    min.z = worldPoint.z;
                }

                if (worldPoint.x > max.x)
                {
                    max.x = worldPoint.x;
                }

                if (worldPoint.y > max.y)
                {
                    max.y = worldPoint.y;
                }

                if (worldPoint.z > max.z)
                {
                    max.z = worldPoint.z;
                }
            }

            if (points.Length == 0)
            {
                min = collider2D.transform.position;
                max = collider2D.transform.position;
            }

            return new Bounds()
            {
                min = min,
                max = max
            };
        }
    }
}