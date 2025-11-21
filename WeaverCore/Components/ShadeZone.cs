using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Playmaker;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Defines a zone where the player's shade will spawn when they die.
    /// If the player dies within the zone's BoxCollider2D, the shade will spawn at the zone's position.
    /// If the player dies outside all zones, the nearest zone will be used.
    /// If there are no zones in the scene, the default Hollow Knight logic is used.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ShadeZone : MonoBehaviour
    {
        private static List<ShadeZone> allZones = new List<ShadeZone>();
        //private static FSMUndoStack heroDeathUndoStack;

        [Header("Shade Spawn Settings")]
        [Tooltip("The position offset from this GameObject's position where the shade will spawn")]
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        //[Header("Gizmo Settings")]
        [Tooltip("Color of the zone bounds when selected")]
        private Color gizmoZoneColor = new Color(1f, 0.5f, 0f, 0.3f);

        [Tooltip("Color of the spawn point when selected")]
        private Color gizmoSpawnColor = new Color(1f, 0f, 0f, 1f);

        [Tooltip("Size of the spawn point gizmo")]
        private float spawnPointSize = 0.5f;

        private BoxCollider2D zoneCollider;

        /// <summary>
        /// Gets the world position where the shade should spawn
        /// </summary>
        public Vector3 ShadeSpawnPosition => (Vector2)transform.position + spawnOffset;

        /// <summary>
        /// Gets all active ShadeZones in the current scene
        /// </summary>
        public static IReadOnlyList<ShadeZone> AllZones => allZones;

        private void Awake()
        {
            zoneCollider = GetComponent<BoxCollider2D>();
            if (zoneCollider == null)
            {
                Debug.LogError($"ShadeZone on {gameObject.name} requires a BoxCollider2D component!", this);
            }
            else
            {
                // Make sure the collider is a trigger
                zoneCollider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            if (!allZones.Contains(this))
            {
                allZones.Add(this);
            }
        }

        private void OnDisable()
        {
            allZones.Remove(this);
        }

        [OnPlayerInit(-10)]
        static void OnPlayerInit(Player player)
        {
            // Only patch if we're in the game and there are zones in the scene
            if (Initialization.Environment != WeaverCore.Enums.RunningState.Game)
            {
                return;
            }

            // Find the Hero Death prefab - it's a child of the player
            Transform heroDeathTransform = player.transform.Find("Hero Death");
            if (heroDeathTransform == null)
            {
                //Debug.LogError("[ShadeZone] Could not find 'Hero Death' prefab in player");
                return;
            }

            // Create the FSMUndoStack for the Hero Death Anim FSM
            //heroDeathUndoStack = new FSMUndoStack(heroDeathTransform.gameObject, "Hero Death Anim");

            var fsm = PlayMakerUtilities.FindPlayMakerFSMWrapper(heroDeathTransform.gameObject, "Hero Death Anim");

            // Get the "Set Shade" state and insert our custom action at the beginning
            if (fsm.GetFsm().TryGetState("Set Shade", out var setState))
            {
                // Insert our custom action at index 0 (before the default shade position logic)
                setState.AddAction(fsm =>
                {
                    // Check if there are any ShadeZones in the scene
                    if (allZones.Count == 0)
                    {
                        // No zones, let default HK logic handle it
                        return;
                    }

                    // Get the player's current position
                    Vector2 playerPosition = Player.Player1.transform.position;

                    // Find the appropriate shade zone
                    ShadeZone targetZone = FindZoneForPosition(playerPosition);

                    if (targetZone != null)
                    {
                        // Override the shade position with the ShadeZone's spawn position
                        Vector3 shadeSpawnPos = targetZone.ShadeSpawnPosition;

                        // Set the shade position in PlayerData
                        // These will be read by SceneManager when spawning the shade
                        PlayerData.instance.SetFloat("shadePositionX", shadeSpawnPos.x);
                        PlayerData.instance.SetFloat("shadePositionY", shadeSpawnPos.y);

                        // Also set the current scene as the shade scene
                        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                        PlayerData.instance.SetString("shadeScene", currentScene);

                        var mapZone = fsm.GetStringVariable("Map Zone");
                        PlayerData.instance.SetString("shadeMapZone", mapZone);

                        Debug.Log($"[ShadeZone] Player died at {playerPosition}, shade will spawn at {shadeSpawnPos}");
                    }
                });

                Debug.Log("[ShadeZone] Successfully patched Hero Death Anim FSM");
            }
            else
            {
                Debug.LogError("[ShadeZone] Could not find 'Set Shade' state in Hero Death Anim FSM");
            }
        }

        /// <summary>
        /// Checks if a world position is inside this zone's bounds
        /// </summary>
        public bool ContainsPoint(Vector2 worldPosition)
        {
            if (zoneCollider == null) return false;

            // Convert world position to local space
            Vector2 localPoint = transform.InverseTransformPoint(worldPosition);

            // Get the collider bounds in local space
            Bounds bounds = new Bounds(zoneCollider.offset, zoneCollider.size);

            return bounds.Contains(localPoint);
        }

        /// <summary>
        /// Gets the distance from a world position to the nearest edge of this zone.
        /// Accounts for the zone's scale, position, and rotation.
        /// </summary>
        public float GetDistanceToZone(Vector2 worldPosition)
        {
            if (zoneCollider == null) return float.MaxValue;

            // Convert world position to local space
            Vector2 localPoint = transform.InverseTransformPoint(worldPosition);

            // Get the collider bounds in local space
            Vector2 colliderOffset = zoneCollider.offset;
            Vector2 halfSize = zoneCollider.size * 0.5f;

            // Calculate the min and max bounds of the box in local space
            Vector2 min = colliderOffset - halfSize;
            Vector2 max = colliderOffset + halfSize;

            // Find the closest point on the box edge to the local point
            Vector2 closestPoint = new Vector2(
                Mathf.Clamp(localPoint.x, min.x, max.x),
                Mathf.Clamp(localPoint.y, min.y, max.y)
            );

            // Convert the closest point back to world space
            Vector2 closestWorldPoint = transform.TransformPoint(closestPoint);

            // Return the distance from the world position to the closest point on the edge
            return Vector2.Distance(worldPosition, closestWorldPoint);
        }

        /// <summary>
        /// Finds the appropriate shade zone for the given player position.
        /// Returns the zone the player is inside, or the nearest zone if outside all zones.
        /// Returns null if there are no zones in the scene.
        /// </summary>
        public static ShadeZone FindZoneForPosition(Vector2 playerPosition)
        {
            if (allZones.Count == 0)
            {
                return null;
            }

            // First, check if the player is inside any zone
            foreach (var zone in allZones)
            {
                if (zone != null && zone.gameObject.activeInHierarchy && zone.ContainsPoint(playerPosition))
                {
                    return zone;
                }
            }

            // If not inside any zone, find the nearest one
            ShadeZone nearestZone = null;
            float nearestDistance = float.MaxValue;

            foreach (var zone in allZones)
            {
                if (zone != null && zone.gameObject.activeInHierarchy)
                {
                    float distance = zone.GetDistanceToZone(playerPosition);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestZone = zone;
                    }
                }
            }

            return nearestZone;
        }

        private void OnDrawGizmosSelected()
        {
            if (zoneCollider == null)
            {
                zoneCollider = GetComponent<BoxCollider2D>();
            }

            if (zoneCollider != null)
            {
                // Draw the zone bounds
                Gizmos.color = gizmoZoneColor;
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;

                Vector3 size = new Vector3(zoneCollider.size.x, zoneCollider.size.y, 0.1f);
                Gizmos.DrawCube(zoneCollider.offset, size);

                // Draw wire cube for outline
                Gizmos.color = new Color(gizmoZoneColor.r, gizmoZoneColor.g, gizmoZoneColor.b, 1f);
                Gizmos.DrawWireCube(zoneCollider.offset, size);

                Gizmos.matrix = oldMatrix;
            }

            // Draw the spawn point
            Vector3 spawnPos = ShadeSpawnPosition;

            // Draw spawn point as a sphere
            Gizmos.color = gizmoSpawnColor;
            Gizmos.DrawSphere(spawnPos, spawnPointSize);

            // Draw wireframe sphere
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(spawnPos, spawnPointSize);

            // Draw a line from zone center to spawn point if there's an offset
            if (spawnOffset != Vector2.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, spawnPos);
            }

            // Draw coordinate labels
            #if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(spawnPos + Vector3.up * 0.5f,
                $"Shade Spawn\n({spawnPos.x:F2}, {spawnPos.y:F2})");
            #endif
        }

        private void OnDrawGizmos()
        {
            // Draw a subtle indicator even when not selected
            if (zoneCollider == null)
            {
                zoneCollider = GetComponent<BoxCollider2D>();
            }

            if (zoneCollider != null)
            {
                Gizmos.color = new Color(gizmoZoneColor.r, gizmoZoneColor.g, gizmoZoneColor.b, 0.1f);
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(zoneCollider.offset, zoneCollider.size);
                Gizmos.matrix = oldMatrix;
            }
        }
    }
}
