using Modding;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverAreaTitleController : AreaTitleController
    {
        [field: SerializeField]
        public string CenterText { get; set; }

        [field: SerializeField]
        public string UpperText { get; set; }

        [field: SerializeField]
        public string LowerText { get; set; }

        [Space]
        [Header("Settings")]
        [SerializeField]
        bool alwaysShowLargeTitle = false;

        [SerializeField]
        [Tooltip("The save specific settings that stores whether or not this area has been visited before")]
        SaveSpecificSettings settings;

        [SerializeField]
        [Tooltip("The bool field on the save specific settings object for storing whether or not this area has been visited before")]
        string saveSettingsVisitedField;

        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var destroyMethod = typeof(AreaTitleController).GetMethod(nameof(OnDestroy),BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var post = typeof(WeaverAreaTitleController).GetMethod(nameof(OnDestroy_Post), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(destroyMethod, null, post);


            var finishMethod = typeof(AreaTitleController).GetMethod("Finish", BindingFlags.Instance | BindingFlags.NonPublic);

            var pre = typeof(WeaverAreaTitleController).GetMethod(nameof(Finish_Pre), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(finishMethod, pre, null);
        }

        static void OnDestroy_Post(AreaTitleController __instance)
        {
            if (__instance is WeaverAreaTitleController wtc)
            {
                ModHooks.LanguageGetHook -= wtc.ModHooks_LanguageGetHook;
            }
        }

        //static void 

        static bool Finish_Pre(AreaTitleController __instance)
        {
            if (__instance is WeaverAreaTitleController wtc)
            {
                bool visited;
                if (wtc.alwaysShowLargeTitle)
                {
                    visited = false;
                }
                else
                {
                    if (wtc.settings.HasField<bool>(wtc.saveSettingsVisitedField))
                    {
                        visited = wtc.settings.GetFieldValue<bool>(wtc.saveSettingsVisitedField);

                        if (!visited)
                        {
                            wtc.settings.SetFieldValue<bool>(wtc.saveSettingsVisitedField, true);
                        }
                    }
                    else
                    {
                        visited = false;
                    }
                }
                if (visited)
                {
                    wtc.StartCoroutine((IEnumerator)typeof(AreaTitleController).GetMethod("VisitPause", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance,new object[] {true}));
                }
                else
                {
                    wtc.StartCoroutine((IEnumerator)typeof(AreaTitleController).GetMethod("UnvisitPause", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { true }));
                }

                return false;
            }
            return true;
        }



        private void OnEnable()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            //WeaverLog.Log("SHEET = " + sheetTitle);
            //WeaverLog.Log("KEY = " + key);
            //areaEvent + "_MAIN"
            //areaEvent + "_SUB"
            //areaEvent + "_SUPER"

            if (sheetTitle == "Titles")
            {
                if (key == $"{areaEvent}_MAIN")
                {
                    return CenterText;
                }
                else if (key == $"{areaEvent}_SUB")
                {
                    return LowerText;
                }
                else if (key == $"{areaEvent}_SUPER")
                {
                    return UpperText;
                }
            }

            return orig;
        }

        private void OnDisable()
        {
            ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        }

        private void Reset()
        {
            unvisitedPause = 0f;
            visitedPause = 0f;
            areaEvent = "AREA_" + Guid.NewGuid().ToString();
            waitForHeroInPosition = true;
            waitForTrigger = true;
        }
    }
}
