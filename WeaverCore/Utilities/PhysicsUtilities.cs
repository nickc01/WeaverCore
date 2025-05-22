using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class PhysicsUtilities
    {
        static RaycastHit2D[] hitCache = new RaycastHit2D[1];
        static List<Vector2> hitPoints = new List<Vector2>();

        static Vector2[] corners = new Vector2[4];

        public static bool PseudoCircleCast2DNonAlloc(Vector2 start, float radius, Vector2 direction, float distance, int layerMask, out Vector2 nearestPoint, int intervals = 4, Func<RaycastHit2D, bool> filter = null)
        {
            hitPoints.Clear();
            var dirAngle = MathUtilities.CartesianToPolar(direction).x;

            //const int DEGREE_INTERVAL = 90;
            float degreeInterval = 360f / intervals;

            for (float i = 0; i < 360; i += degreeInterval)
            {
                var currentAngle = dirAngle + i;
                var currentDirOffset = MathUtilities.PolarToCartesian(currentAngle, radius);
                var startPoint = start + currentDirOffset;

                Debug.DrawRay(startPoint, direction * distance, Color.Lerp(Color.blue, Color.red, 0.5f), 0);
                if (Physics2D.RaycastNonAlloc(startPoint, direction, hitCache, distance, layerMask, -900) > 0 && (filter == null || filter(hitCache[0])))
                {
                    hitPoints.Add(hitCache[0].point - currentDirOffset);
                }
                else
                {
                    //hitPoints.Add(start + direction * distance);
                }
            }

            float minDistance = float.PositiveInfinity;
            nearestPoint = default;

            foreach (var hitPoint in hitPoints)
            {
                float dist = Vector2.Distance(start, hitPoint);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestPoint = hitPoint;
                }
            }

            return !float.IsInfinity(minDistance);
        }

        public static bool PseudoBoxCast2DNonAlloc(Vector2 center, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, out Vector2 nearestPoint, Func<RaycastHit2D, bool> filter = null)
        {
            hitPoints.Clear();
            Vector2 halfSize = size * 0.5f;
            corners[0] = center + RotatePoint(new Vector2(-halfSize.x, -halfSize.y), angle);
            corners[1] = center + RotatePoint(new Vector2(halfSize.x, -halfSize.y), angle);
            corners[2] = center + RotatePoint(new Vector2(halfSize.x, halfSize.y), angle);
            corners[3] = center + RotatePoint(new Vector2(-halfSize.x, halfSize.y), angle);

            // Cast ray from each corner in the given direction
            for (int i = 0; i < corners.Length; i++)
            {
                //Debug.DrawRay(corners[i], direction * distance, Color.Lerp(Color.red, Color.yellow, 0.5f), 1f);
                if (Physics2D.RaycastNonAlloc(corners[i], direction, hitCache, distance, layerMask, -900) > 0 && (filter == null || filter(hitCache[0])))
                {
                    Debug.DrawLine(corners[i], hitCache[0].point - (corners[i] - center), Color.Lerp(Color.red, Color.yellow, 0.5f), 1f);
                    hitPoints.Add(hitCache[0].point - (corners[i] - center));
                }
                else
                {
                    //hitPoints.Add(center + direction * distance);
                }
            }

            // Find the nearest hit point
            nearestPoint = default;
            float minDistance = float.PositiveInfinity;

            foreach (var hitPoint in hitPoints)
            {
                float dist = Vector2.Distance(center, hitPoint);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestPoint = hitPoint;
                }
            }

            return !float.IsInfinity(minDistance);
        }

        // Optimized rotation when pivot is (0,0)
        static Vector2 RotatePoint(Vector2 point, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad);
            float cos = Mathf.Cos(rad);

            // Apply rotation matrix
            return new Vector2(
                point.x * cos - point.y * sin,
                point.x * sin + point.y * cos
            );
        }

        public static Vector2 GetClosestContactPointOnCircle(CircleCollider2D circle, Collider2D other)
        {
            if (circle == null) throw new ArgumentNullException(nameof(circle));
            if (other == null) throw new ArgumentNullException(nameof(other));

            ColliderDistance2D distInfo = Physics2D.Distance(circle, other);

            if (distInfo.isValid)
            {
                return distInfo.pointA;
            }

            Vector2 circleCenter = circle.transform.TransformPoint(circle.offset);
            Vector2 closestOnOther = other.ClosestPoint(circleCenter);

            Vector2 dir = closestOnOther - circleCenter;
            if (dir.sqrMagnitude < Mathf.Epsilon) dir = Vector2.right;

            float worldRadius =
                circle.radius *
                Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

            return circleCenter + dir.normalized * worldRadius;
        }

        public static void GetClosestContactAndNormal(CircleCollider2D circle, Collider2D other, out Vector2 pointOnCircle, out Vector2 surfaceNormalOnOther)
        {
            if (circle == null) throw new ArgumentNullException(nameof(circle));
            if (other == null) throw new ArgumentNullException(nameof(other));

            ColliderDistance2D d = Physics2D.Distance(circle, other);

            if (d.isValid)
            {
                pointOnCircle = d.pointA;
                surfaceNormalOnOther = -d.normal;
                return;
            }

            Vector2 circleCenter = circle.transform.TransformPoint(circle.offset);
            Vector2 closestOnOther = other.ClosestPoint(circleCenter);

            Vector2 dir = closestOnOther - circleCenter;
            if (dir.sqrMagnitude < Mathf.Epsilon)
                dir = Vector2.right;

            float worldRadius =
                circle.radius *
                Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);

            pointOnCircle = circleCenter + dir.normalized * worldRadius;
            surfaceNormalOnOther = (circleCenter - closestOnOther).normalized;
        }
        
        public static Vector2 ReflectWithLimiter(Vector2 incoming, Vector2 surfaceNormal, float angleLimiterDeg)
        {
            if (incoming.sqrMagnitude < Mathf.Epsilon)
                return incoming;

            Vector2 n = surfaceNormal.normalized;
            Vector2 reflected = Vector2.Reflect(incoming, n);

            float signedAngleToNormal = Vector2.SignedAngle(n, reflected);
            float absAngleToNormal = Mathf.Abs(signedAngleToNormal);
            float angleToPlane = 90f - absAngleToNormal;

            if (angleToPlane < angleLimiterDeg)
            {
                float targetAbsAngleToNormal = 90f - angleLimiterDeg;

                float desiredSignedAngle = Mathf.Sign(signedAngleToNormal) * targetAbsAngleToNormal;

                float delta = desiredSignedAngle - signedAngleToNormal;

                reflected = Quaternion.AngleAxis(delta, Vector3.forward) * reflected;
            }

            return reflected;
        }
    }
}