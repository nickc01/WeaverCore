using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Attributes;
using System.Reflection;




#if UNITY_EDITOR
#endif

namespace WeaverCore.Components.Colosseum
{
    public class FallbackSpawnLocation : MonoBehaviour, IColosseumIdentifier
    {
        string IColosseumIdentifier.Identifier => "Fallback Spawn Location";

        Color IColosseumIdentifier.Color => Color.Lerp(Color.red, Color.black, 0.3f);

        bool IColosseumIdentifier.ShowShortcut => true;

        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            var orig = typeof(HeroController).GetMethod(nameof(HeroController.HazardRespawn));
            var prefix = typeof(FallbackSpawnLocation).GetMethod(nameof(HazardRespawn_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }

        static bool HazardRespawn_Prefix(HeroController __instance)
        {
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
            
            Vector3 playerPos = __instance.transform.position;
            
            System.Array.Sort(fallbackLocations, (a, b) => 
                Vector3.Distance(playerPos, a.transform.position).CompareTo(Vector3.Distance(playerPos, b.transform.position)));
            
            foreach (var location in fallbackLocations)
            {
                if (TryFindGroundPoint(location.transform.position, out Vector3 groundPoint))
                {
                    __instance.SetHazardRespawn(groundPoint, true);
                    return true;
                }
            }
            
            return true;
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
    }
}