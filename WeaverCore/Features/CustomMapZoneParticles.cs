using GlobalEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
    [ShowFeature]
    [CreateAssetMenu(fileName = "Custom Map Zone Particles", menuName = "WeaverCore/Custom Map Zone Particles")]
    public class CustomMapZoneParticles : ScriptableObject
    {
        public GameObject ParticlesPrefab;
        public MapZone MapZone;

        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(SceneParticlesController).GetMethod(nameof(SceneParticlesController.SceneInit));
                var prefix = typeof(CustomMapZoneParticles).GetMethod(nameof(SceneInitPrefix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(orig, prefix, null);
            }
        }

        static bool SceneInitPrefix(SceneParticlesController __instance)
        {
            WeaverLog.Log("LOADING SCENE INIT");
            foreach (var registry in GetAllRegistries())
            {
                foreach (var custom in registry.GetFeatures<CustomMapZoneParticles>())
                {
                    WeaverLog.Log("FOUND CUSTOM = " + custom.MapZone);
                    if (custom.ParticlesPrefab != null)
                    {
                        var particleIndex = FindIndexIn(__instance.sceneParticles, v => v.mapZone == custom.MapZone);

                        if (particleIndex >= 0)
                        {
                            var origInstance = __instance.sceneParticles[particleIndex].particleObject;

                            if (origInstance != null && origInstance.gameObject.name != $"{custom.ParticlesPrefab.name}_CUSTOM")
                            {
                                GameObject.Destroy(origInstance);
                                var newInstance = GameObject.Instantiate(custom.ParticlesPrefab, __instance.transform);
                                newInstance.transform.localPosition = Vector3.zero;
                                newInstance.gameObject.SetActive(false);
                                newInstance.gameObject.name = $"{custom.ParticlesPrefab.name}_CUSTOM";
                                __instance.sceneParticles[particleIndex].particleObject = newInstance;
                            }
                        }
                        else
                        {
                            var newInstance = GameObject.Instantiate(custom.ParticlesPrefab, __instance.transform);
                            newInstance.transform.localPosition = Vector3.zero;
                            newInstance.gameObject.SetActive(false);
                            newInstance.gameObject.name = $"{custom.ParticlesPrefab.name}_CUSTOM";

                            Array.Resize(ref __instance.sceneParticles, __instance.sceneParticles.Length + 1);
                            __instance.sceneParticles[__instance.sceneParticles.Length - 1] = new SceneParticles
                            {
                                mapZone = custom.MapZone,
                                particleObject = newInstance
                            };
                        }
                    }
                }
            }

            return true;
        }

        static IEnumerable<Registry> GetAllRegistries()
        {
#if UNITY_EDITOR
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Registry"))
            {
                yield return UnityEditor.AssetDatabase.LoadAssetAtPath<Registry>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
            }
#else
            return Registry.AllRegistries;
#endif
        }

        static int FindIndexIn<T>(T[] array, Predicate<T> predicate)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
