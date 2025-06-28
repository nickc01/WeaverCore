using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Attributes;
using System.Reflection;
using System.Collections;




#if UNITY_EDITOR
#endif

namespace WeaverCore.Components.Colosseum
{
    public class FallbackSpawnLocation : MonoBehaviour, IColosseumIdentifier
    {
        [SerializeField]
        ColosseumPlatform platformPrefab;

        [SerializeField]
        Vector2 platformSpawnOffset = new Vector2(0f, -2f);

        [SerializeField]
        bool spawnSafetyPlatform = true;

        [SerializeField]
        float platformExpandTimeMultiplier = 0.5f;

        [SerializeField]
        float platformSpawnPreDelay = 0f;

        [SerializeField]
        float platformSpawnDuration = 1f;

        [SerializeField]
        float platformEndDelay = 3f;

        [SerializeField]
        bool checkForHazardsBelow = false;

        string IColosseumIdentifier.Identifier => "Fallback Spawn Location";

        Color IColosseumIdentifier.Color => Color.Lerp(Color.red, Color.black, 0.3f);

        bool IColosseumIdentifier.ShowShortcut => true;

        private void Awake() {
            if (platformPrefab == null)
            {
                platformPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Colosseum Platform").GetComponent<ColosseumPlatform>();
            }
        }

        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            var orig = typeof(GameManager).GetMethod("PlayerDeadFromHazard");
            var prefix = typeof(FallbackSpawnLocation).GetMethod(nameof(HazardRespawn_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }

        static bool HazardRespawn_Prefix()
        {
            var oldState = Physics2D.queriesHitTriggers;
            try
            {
                Physics2D.queriesHitTriggers = true;
                Vector3 currentRespawnLocation = PlayerData.instance.hazardRespawnLocation;

                bool isCurrentLocationSafe = TryFindGroundPoint(currentRespawnLocation, out Vector3 _);

                if (isCurrentLocationSafe)
                {
                    return true;
                }

                var fallbackLocations = FindObjectsOfType<FallbackSpawnLocation>(false);
                if (fallbackLocations.Length == 0)
                {
                    return true;
                }

                Vector3 playerPos = HeroController.instance.transform.position;

                System.Array.Sort(fallbackLocations, (a, b) =>
                    Vector3.Distance(playerPos, a.transform.position).CompareTo(Vector3.Distance(playerPos, b.transform.position)));

                foreach (var location in fallbackLocations)
                {
                    if (!location.checkForHazardsBelow || (location.checkForHazardsBelow && TryFindGroundPoint(location.transform.position, out Vector3 groundPoint)))
                    {
                        HeroController.instance.SetHazardRespawn(location.transform.position, true);
                        if (location.spawnSafetyPlatform && location.platformPrefab != null)
                        {
                            WeaverLog.Log("SPAWNING SAFETY PLATFORM");
                            GameManager.instance.StartCoroutine(location.SpawnPlatform());
                        }
                        return true;
                    }
                }

                return true;
            }
            finally
            {
                Physics2D.queriesHitTriggers = false;
            }
        }

        public static bool TryFindGroundPoint(Vector2 startPoint, out Vector3 grounPoint, bool useExtended = false)
        {
            float num = HeroController.instance.ReflectGetField<float>("FIND_GROUND_POINT_DISTANCE");
            if (useExtended)
            {
                num = HeroController.instance.ReflectGetField<float>("FIND_GROUND_POINT_DISTANCE_EXT");
            }

            grounPoint = Vector3.zero;

            var cache = HitCache.GetMultiCachedArray(10);
            LayerMask terrainMask = LayerMask.GetMask("Terrain");
            LayerMask obstacleMask = LayerMask.GetMask("Attack", "Enemy Attack");

            int hits = Physics2D.RaycastNonAlloc(startPoint, Vector2.down, cache, num, terrainMask);

            for (int i = 0; i < hits; i++)
            {
                var hit = cache[i];
                Vector2 groundPoint = hit.point;

                int obstacleHits = Physics2D.RaycastNonAlloc(startPoint, (groundPoint - startPoint).normalized, cache, Vector2.Distance(startPoint, groundPoint), obstacleMask, -900);

                if (obstacleHits == 0)
                {
                    grounPoint = groundPoint;
                    return true;
                }
            }

            return false;
        }

        IEnumerator SpawnPlatform()
        {
            if (platformSpawnPreDelay > 0)
            {
                yield return new WaitForSeconds(platformSpawnPreDelay);
            }

            if (this == null)
            {
                yield break;
            }

            var instance = GameObject.Instantiate(platformPrefab, transform.position + (Vector3)platformSpawnOffset, Quaternion.identity);
            instance.Retract();
            instance.INSTANT_TIME = -1;
            instance.expandTime *= platformExpandTimeMultiplier;
            instance.Expand();

            yield return new WaitForSeconds(platformSpawnDuration);

            if (this == null || instance == null)
            {
                yield break;
            }
            instance.RetractSlowWithDelay(platformEndDelay);

            yield return new WaitForSeconds(platformEndDelay + 5f);

            if (this == null || instance == null)
            {
                yield break;
            }

            GameObject.Destroy(instance.gameObject);
        }
    }
}