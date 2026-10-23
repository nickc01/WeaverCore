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
            if (Initialization.Environment != WeaverCore.Enums.RunningState.Game)
            {
                return;
            }

            Transform heroDeathTransform = player.transform.Find("Hero Death");
            if (heroDeathTransform == null)
            {
                return;
            }

            var fsm = PlayMakerUtilities.FindPlayMakerFSMWrapper(heroDeathTransform.gameObject, "Hero Death Anim");
            var deathFsm = fsm.GetFsm();
            const string zoneStateName = "Apply Shade Zone";

            if (deathFsm.TryGetState(zoneStateName, out _))
            {
                return;
            }

            if (!deathFsm.TryGetState("Shade Marker Check", out var markerCheck))
            {
                Debug.LogError("[ShadeZone] Could not find 'Shade Marker Check' state in Hero Death Anim FSM");
                return;
            }

            var transitions = markerCheck.GetTransitions();
            for (int i = 0; i < transitions.Length; i++)
            {
                if (transitions[i].FsmEvent.Name != "FINISHED")
                {
                    continue;
                }

                var zoneState = deathFsm.AddState(zoneStateName, Vector2.zero);
                zoneState.AddAction(() =>
                {
                    var targetZone = FindZoneForPosition(player.transform.position);
                    if (targetZone == null)
                    {
                        return;
                    }

                    Vector3 shadeSpawnPos = targetZone.ShadeSpawnPosition;
                    PlayerData.instance.SetFloat("shadePositionX", shadeSpawnPos.x);
                    PlayerData.instance.SetFloat("shadePositionY", shadeSpawnPos.y);
                    PlayerData.instance.SetString("shadeScene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                    PlayerData.instance.SetString("shadeMapZone", deathFsm.GetStringVariable("Map Zone"));
                });
                zoneState.AddTransition("FINISHED", transitions[i].ToState);
                markerCheck.RemoveTransition(i);
                markerCheck.AddTransition("FINISHED", zoneStateName);
                return;
            }

            Debug.LogError("[ShadeZone] Could not find 'FINISHED' transition in Shade Marker Check");
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
