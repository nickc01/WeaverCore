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
    /// <summary>
    /// Controller for managing area titles in the game.
    /// </summary>
    public class WeaverAreaTitleController : AreaTitleController
    {
        /// <summary>
        /// Gets or sets the center text for the area title.
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("The center text for the area title.")]
        public string CenterText { get; set; }

        /// <summary>
        /// Gets or sets the upper text for the area title.
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("The upper text for the area title.")]
        public string UpperText { get; set; }

        /// <summary>
        /// Gets or sets the lower text for the area title.
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("The lower text for the area title.")]
        public string LowerText { get; set; }

        [Space]
        [Header("Settings")]

        /// <summary>
        /// Determines whether the large title should always be shown.
        /// </summary>
        [SerializeField]
        [Tooltip("Determines whether the large title should always be shown.")]
        bool alwaysShowLargeTitle = false;

        /// <summary>
        /// The save specific settings that store whether or not this area has been visited before.
        /// </summary>
        [SerializeField]
        [Tooltip("The save specific settings that store whether or not this area has been visited before.")]
        SaveSpecificSettings settings;

        /// <summary>
        /// The bool field on the save specific settings object for storing whether or not this area has been visited before.
        /// </summary>
        [SerializeField]
        [Tooltip("The bool field on the save specific settings object for storing whether or not this area has been visited before.")]
        [SaveSpecificFieldName(typeof(bool), nameof(settings))]
        string saveSettingsVisitedField;

        /// <summary>
        /// Harmony patch method for OnDestroy.
        /// </summary>
        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var destroyMethod = typeof(AreaTitleController).GetMethod(nameof(OnDestroy), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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
                    wtc.StartCoroutine((IEnumerator)typeof(AreaTitleController).GetMethod("VisitPause", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { true }));
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
