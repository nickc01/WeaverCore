using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to Colliders
    /// </summary>
    public static class ColliderUtilities
    {
        static List<Vector2> pointsCache = new List<Vector2>();

        public static Bounds GetBoundsSafe(this BoxCollider2D collider2D)
        {
            return BoxBounds(collider2D, collider2D.offset, collider2D.size);
            //return new Bounds((Vector2)collider2D.transform.TransformPoint(collider2D.offset), (Vector2)collider2D.size);
        }

        static Bounds BoxBounds(Collider2D collider2D, Vector2 offset, Vector2 size)
        {
            var halfSize = size / 2f;
            var min = (Vector2)collider2D.transform.TransformPoint(offset - halfSize);
            var max = (Vector2)collider2D.transform.TransformPoint(offset + halfSize);

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
        }

        public static Bounds GetBoundsSafe(this CircleCollider2D collider2D)
        {
            //var pos = collider2D.transform.TransformPoint(collider2D.offset);
            return CircleBounds(collider2D, collider2D.offset, collider2D.radius);
        }

        static Bounds CircleBounds(Collider2D collider2D, Vector2 offset, float radius)
        {
            var min = collider2D.transform.TransformPoint(collider2D.offset - new Vector2(radius, radius));
            var max = collider2D.transform.TransformPoint(collider2D.offset + new Vector2(radius, radius));
            return new Bounds
            {
                min = min,
                max = max
            };
        }

        public static bool GetBoundsSafe(this Collider2D collider2D, out Bounds bounds)
        {
            if (collider2D is BoxCollider2D box)
            {
                bounds = GetBoundsSafe(box);
                return true;
            }
            else if (collider2D is CircleCollider2D circle)
            {
                bounds = GetBoundsSafe(circle);
                return true;
            }
            else if (collider2D is CapsuleCollider2D capsule)
            {
                bounds = GetBoundsSafe(capsule);
                return true;
            }
            else if (collider2D is PolygonCollider2D poly)
            {
                bounds = GetBoundsSafe(poly);
                return true;
            }
            else if (collider2D is EdgeCollider2D edge)
            {
                bounds = GetBoundsSafe(edge);
                return true;
            }
            else
            {
                bounds = default;
                return false;
            }
        }

        public static Bounds GetBoundsSafe(this Collider2D collider2D)
        {
            if (collider2D is BoxCollider2D box)
            {
                return GetBoundsSafe(box);
            }
            else if (collider2D is CircleCollider2D circle)
            {
                return GetBoundsSafe(circle);
            }
            else if (collider2D is CapsuleCollider2D capsule)
            {
                return GetBoundsSafe(capsule);
            }
            else if (collider2D is PolygonCollider2D poly)
            {
                return GetBoundsSafe(poly);
            }
            else if (collider2D is EdgeCollider2D edge)
            {
                return GetBoundsSafe(edge);
            }
            else
            {
                throw new System.Exception($"The collider of type {collider2D.GetType().Name} is not supported by GetBoundsSafe");
            }
        }

        public static Bounds GetBoundsSafe(this CapsuleCollider2D collider2D)
        {
            if (collider2D.direction == CapsuleDirection2D.Horizontal)
            {
                if (collider2D.size.y >= collider2D.size.x)
                {
                    return CircleBounds(collider2D, collider2D.offset, collider2D.size.y);
                }
                else
                {
                    return BoxBounds(collider2D, collider2D.offset, collider2D.size);
                }

            }
            else
            {
                if (collider2D.size.y <= collider2D.size.x)
                {
                    return CircleBounds(collider2D, collider2D.offset, collider2D.size.x);
                }
                else
                {
                    return BoxBounds(collider2D, collider2D.offset, collider2D.size);
                }
            }
        }

        public static Bounds GetBoundsSafe(this EdgeCollider2D collider2D)
        {
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            //var points = collider2D.points;
            int pointCount = collider2D.GetPoints(pointsCache);

            for (int i = 0; i < pointCount; i++)
            {
                var worldPoint = collider2D.transform.TransformPoint(pointsCache[i]);

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

            if (pointCount == 0)
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

        /// <summary>
        /// Gets the world space boundaries of a polygon collider
        /// </summary>
        /// <param name="collider2D">The collider to get the bounds of</param>
        /// <returns>Returns the boundaries of the polygon collider</returns>
        public static Bounds GetBoundsSafe(this PolygonCollider2D collider2D)
        {
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

            //var points = collider2D.points;
            int pointCount = collider2D.GetPath(0, pointsCache);

            for (int i = 0; i < pointCount; i++)
            {
                var worldPoint = collider2D.transform.TransformPoint(pointsCache[i]);

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

            if (pointCount == 0)
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