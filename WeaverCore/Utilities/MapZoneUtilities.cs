

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