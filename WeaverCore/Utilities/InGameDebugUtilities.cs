using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Utilities for drawing debug visuals that render in-game instead of just in the Scene view
    /// </summary>
    public static class InGameDebugUtilities
    {
        private static readonly Dictionary<int, GameObject> debugObjects = new Dictionary<int, GameObject>();
        private static int nextDebugId = 0;
        private static GameObject debugParent;

        private static GameObject GetOrCreateDebugParent()
        {
            if (debugParent == null)
            {
                debugParent = new GameObject("InGameDebugUtilities");
                Object.DontDestroyOnLoad(debugParent);
            }
            return debugParent;
        }

        /// <summary>
        /// Draws a line in-game between two points
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="color">Line color</param>
        /// <param name="duration">How long to display the line (0 = one frame)</param>
        /// <param name="width">Line width</param>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, float width = 0.02f)
        {
            var lineObj = CreateDebugLine(start, end, color, width);
            
            if (duration > 0f)
            {
                var destroyer = lineObj.AddComponent<TimedDestroyer>();
                destroyer.timeToDestroy = duration;
            }
            else
            {
                Object.Destroy(lineObj, Time.deltaTime);
            }
        }

        /// <summary>
        /// Draws a ray in-game from a starting point in a direction
        /// </summary>
        /// <param name="start">Ray origin</param>
        /// <param name="direction">Ray direction</param>
        /// <param name="color">Ray color</param>
        /// <param name="duration">How long to display the ray (0 = one frame)</param>
        /// <param name="length">Ray length</param>
        /// <param name="width">Ray width</param>
        public static void DrawRay(Vector3 start, Vector3 direction, Color color, float duration = 0f, float length = 1f, float width = 0.02f)
        {
            DrawLine(start, start + direction.normalized * length, color, duration, width);
        }

        /// <summary>
        /// Draws a sphere wireframe in-game
        /// </summary>
        /// <param name="center">Sphere center</param>
        /// <param name="radius">Sphere radius</param>
        /// <param name="color">Sphere color</param>
        /// <param name="duration">How long to display the sphere (0 = one frame)</param>
        /// <param name="segments">Number of segments for sphere detail</param>
        public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration = 0f, int segments = 16)
        {
            var sphereObj = CreateDebugWireSphere(center, radius, color, segments);
            
            if (duration > 0f)
            {
                var destroyer = sphereObj.AddComponent<TimedDestroyer>();
                destroyer.timeToDestroy = duration;
            }
            else
            {
                Object.Destroy(sphereObj, Time.deltaTime);
            }
        }

        /// <summary>
        /// Draws a cube wireframe in-game
        /// </summary>
        /// <param name="center">Cube center</param>
        /// <param name="size">Cube size</param>
        /// <param name="color">Cube color</param>
        /// <param name="duration">How long to display the cube (0 = one frame)</param>
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color, float duration = 0f)
        {
            var cubeObj = CreateDebugWireCube(center, size, color);
            
            if (duration > 0f)
            {
                var destroyer = cubeObj.AddComponent<TimedDestroyer>();
                destroyer.timeToDestroy = duration;
            }
            else
            {
                Object.Destroy(cubeObj, Time.deltaTime);
            }
        }

        /// <summary>
        /// Draws collision bounds for a BoxCollider2D
        /// </summary>
        /// <param name="collider">The BoxCollider2D to visualize</param>
        /// <param name="color">Bounds color</param>
        /// <param name="duration">How long to display the bounds (0 = one frame)</param>
        public static void DrawColliderBounds(BoxCollider2D collider, Color color, float duration = 0f)
        {
            if (collider == null) return;

            Vector2 center = (Vector2)collider.transform.position + collider.offset;
            Vector2 size = collider.size;
            
            DrawWireCube(center, size, color, duration);
        }

        private static GameObject CreateDebugLine(Vector3 start, Vector3 end, Color color, float width)
        {
            var lineObj = new GameObject($"DebugLine_{nextDebugId++}");
            lineObj.transform.SetParent(GetOrCreateDebugParent().transform);
            
            var lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = CreateDebugMaterial(color);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 1000;
            
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            
            return lineObj;
        }

        private static GameObject CreateDebugWireSphere(Vector3 center, float radius, Color color, int segments)
        {
            var sphereObj = new GameObject($"DebugWireSphere_{nextDebugId++}");
            sphereObj.transform.SetParent(GetOrCreateDebugParent().transform);
            sphereObj.transform.position = center;

            // Create three circles for XY, XZ, and YZ planes
            CreateCircle(sphereObj, Vector3.zero, Vector3.forward, radius, color, segments);
            CreateCircle(sphereObj, Vector3.zero, Vector3.up, radius, color, segments);
            CreateCircle(sphereObj, Vector3.zero, Vector3.right, radius, color, segments);

            return sphereObj;
        }

        private static void CreateCircle(GameObject parent, Vector3 center, Vector3 normal, float radius, Color color, int segments)
        {
            var circleObj = new GameObject("Circle");
            circleObj.transform.SetParent(parent.transform);
            circleObj.transform.localPosition = center;

            var lineRenderer = circleObj.AddComponent<LineRenderer>();
            lineRenderer.material = CreateDebugMaterial(color);
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.useWorldSpace = false;
            lineRenderer.sortingOrder = 1000;

            Vector3 forward = Vector3.Cross(normal, Vector3.up).normalized;
            if (forward.magnitude < 0.1f)
                forward = Vector3.Cross(normal, Vector3.right).normalized;
            Vector3 right = Vector3.Cross(normal, forward).normalized;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                Vector3 pos = center + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * radius;
                lineRenderer.SetPosition(i, pos);
            }
        }

        private static GameObject CreateDebugWireCube(Vector3 center, Vector3 size, Color color)
        {
            var cubeObj = new GameObject($"DebugWireCube_{nextDebugId++}");
            cubeObj.transform.SetParent(GetOrCreateDebugParent().transform);
            cubeObj.transform.position = center;

            var halfSize = size * 0.5f;
            
            // Create 12 lines for cube edges
            Vector3[] corners = new Vector3[]
            {
                center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z), // 0: bottom-left-back
                center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),  // 1: bottom-right-back
                center + new Vector3(halfSize.x, halfSize.y, -halfSize.z),   // 2: top-right-back
                center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),  // 3: top-left-back
                center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),  // 4: bottom-left-front
                center + new Vector3(halfSize.x, -halfSize.y, halfSize.z),   // 5: bottom-right-front
                center + new Vector3(halfSize.x, halfSize.y, halfSize.z),    // 6: top-right-front
                center + new Vector3(-halfSize.x, halfSize.y, halfSize.z)    // 7: top-left-front
            };

            // Bottom face
            CreateDebugLineChild(cubeObj, corners[0], corners[1], color);
            CreateDebugLineChild(cubeObj, corners[1], corners[2], color);
            CreateDebugLineChild(cubeObj, corners[2], corners[3], color);
            CreateDebugLineChild(cubeObj, corners[3], corners[0], color);

            // Top face
            CreateDebugLineChild(cubeObj, corners[4], corners[5], color);
            CreateDebugLineChild(cubeObj, corners[5], corners[6], color);
            CreateDebugLineChild(cubeObj, corners[6], corners[7], color);
            CreateDebugLineChild(cubeObj, corners[7], corners[4], color);

            // Vertical edges
            CreateDebugLineChild(cubeObj, corners[0], corners[4], color);
            CreateDebugLineChild(cubeObj, corners[1], corners[5], color);
            CreateDebugLineChild(cubeObj, corners[2], corners[6], color);
            CreateDebugLineChild(cubeObj, corners[3], corners[7], color);

            return cubeObj;
        }

        private static void CreateDebugLineChild(GameObject parent, Vector3 start, Vector3 end, Color color)
        {
            var lineObj = new GameObject("Line");
            lineObj.transform.SetParent(parent.transform);
            
            var lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = CreateDebugMaterial(color);
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 1000;
            
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        private static Material CreateDebugMaterial(Color color)
        {
            var material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            return material;
        }

        /// <summary>
        /// Clears all debug visuals
        /// </summary>
        public static void ClearAll()
        {
            if (debugParent != null)
            {
                Object.Destroy(debugParent);
                debugParent = null;
            }
            debugObjects.Clear();
            nextDebugId = 0;
        }
    }

    /// <summary>
    /// Component that destroys its GameObject after a specified time
    /// </summary>
    public class TimedDestroyer : MonoBehaviour
    {
        public float timeToDestroy;
        private float startTime;

        void Start()
        {
            startTime = Time.time;
        }

        void Update()
        {
            if (Time.time >= startTime + timeToDestroy)
            {
                Destroy(gameObject);
            }
        }
    }
}