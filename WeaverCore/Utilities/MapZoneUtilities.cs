

using GlobalEnums;
using System.Collections.Generic;
using System;
using System.Linq;
using WeaverCore.Attributes;
using Modding;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WeaverCore.Features;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains some utility functions related to Map Zones
    /// </summary>
    public static class MapZoneUtilities
    {
        static bool cacheDirty = true;

        static int[] _mapZoneIndexes = null;
        static string[] _mapZoneNames = null;

        static List<CustomMapZone> customMapZonesCached = null;

        static IEnumerable<CustomMapZone> GetCustomMapZonesInternal()
        {
            IEnumerable<Registry> registries;

#if UNITY_EDITOR
            registries = AssetDatabase.FindAssets("t:Registry").Select(guid => AssetDatabase.LoadAssetAtPath<Registry>(AssetDatabase.GUIDToAssetPath(guid)));
#else
                registries = Registry.AllRegistries;
#endif
            return registries.SelectMany(r => r.GetFeatures<CustomMapZone>());
        }

        /// <summary>
        /// Gets a list of all the custom map zones
        /// </summary>
        /// <returns>Returns a list of all the custom map zones</returns>
        public static List<CustomMapZone> GetCustomMapZones()
        {
            if (cacheDirty || customMapZonesCached == null)
            {
                customMapZonesCached = new List<CustomMapZone>(GetCustomMapZonesInternal());
                cacheDirty = false;
            }
            return customMapZonesCached;
        }

        [OnRegistryLoad]
        static void OnRegistryLoad(Registry r)
        {
            cacheDirty = true;
        }

        [OnRegistryLoad]
        static void OnRegistryUnload(Registry r)
        {
            cacheDirty = true;
        }

        /// <summary>
        /// Gets a list of all the map zones in the game (including custom ones)
        /// </summary>
        /// <param name="mapZoneIndexes">The ID of each of the map zones. These also serve as indexes within the <see cref="MapZone"/> enum, and can be casted to a <see cref="MapZone"/></param>
        /// <param name="mapZoneNames">The internal names of each map zone.</param>
        public static void GetAllMapZones(out int[] mapZoneIndexes, out string[] mapZoneNames)
        {
            if (_mapZoneIndexes == null)
            {
                var mapZoneIndexList = new List<int>();
                var mapZoneNameList = new List<string>();

                foreach (var val in Enum.GetValues(typeof(MapZone)))
                {
                    var enumVal = (MapZone)val;

                    mapZoneIndexList.Add((int)enumVal);
                    mapZoneNameList.Add(enumVal.ToString());
                }

                foreach (var customZone in GetCustomMapZones())
                {
                    var existingIndex = mapZoneIndexList.IndexOf(customZone.MapZoneID);

                    if (existingIndex >= 0)
                    {
                        mapZoneIndexList[existingIndex] = customZone.MapZoneID;
                        mapZoneNameList[existingIndex] = customZone.GetInternalName();
                    }
                    else
                    {
                        mapZoneIndexList.Add(customZone.MapZoneID);
                        mapZoneNameList.Add(customZone.GetInternalName());
                    }
                }

                _mapZoneIndexes = mapZoneIndexList.ToArray();
                _mapZoneNames = mapZoneNameList.ToArray();
            }

            mapZoneIndexes = _mapZoneIndexes;
            mapZoneNames = _mapZoneNames;
        }

        [OnInit]
        static void Init()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (sheetTitle == "Map Zones")
            {
                WeaverLog.Log("MAP ZONE KEY = " + key);
                foreach (var zone in GetCustomMapZones())
                {
                    if (zone.GetInternalName() == key)
                    {
                        return zone.MapZoneName;
                    }
                }
            }

            return orig;
        }
    }
}