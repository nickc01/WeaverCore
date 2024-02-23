using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to Colliders
    /// </summary>
    public static class ColliderUtilities
    {
        public static Bounds GetBoundsSafe(this BoxCollider2D collider2D)
        {
            var halfSize = collider2D.size / 2f;
            var min = (Vector2)collider2D.transform.TransformPoint(collider2D.offset - halfSize);
            var max = (Vector2)collider2D.transform.TransformPoint(collider2D.offset + halfSize);

            if (max.x < min.x)
            {
                var temp = max.x;
                max.x = min.x;
                min.x = temp;
            }

            if (max.y < min.y)
            {
                var temp = max.y;
                max.y = min.y;
                min.y = temp;
            }

            return new Bounds
            {
                min = min,
                max = max
            };

            //return new Bounds((Vector2)collider2D.transform.TransformPoint(collider2D.offset), (Vector2)collider2D.size);
        }

        /// <summary>
        /// Gets the world space boundaries of a polygon collider
        /// </summary>
        /// <param name="collider2D">The collider to get the bounds of</param>
        /// <returns>Returns the boundaries of the polygon collider</returns>
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