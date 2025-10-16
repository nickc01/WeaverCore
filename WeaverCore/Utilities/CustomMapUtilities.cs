using GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions for managing custom maps and their rooms
    /// </summary>
    public static class CustomMapUtilities
    {
        static bool cacheDirty = true;
        static List<CustomMapData> customMapsCached = null;

        static IEnumerable<CustomMapData> GetCustomMapsInternal()
        {
            IEnumerable<Registry> registries;

#if UNITY_EDITOR
            registries = AssetDatabase.FindAssets("t:Registry").Select(guid => AssetDatabase.LoadAssetAtPath<Registry>(AssetDatabase.GUIDToAssetPath(guid)));
#else
            registries = Registry.AllRegistries;
#endif
            return registries.SelectMany(r => r.GetFeatures<CustomMapData>());
        }

        /// <summary>
        /// Gets a list of all custom map data
        /// </summary>
        /// <returns>A list of all custom map data</returns>
        public static List<CustomMapData> GetCustomMaps()
        {
            if (cacheDirty || customMapsCached == null)
            {
                customMapsCached = new List<CustomMapData>(GetCustomMapsInternal());
                cacheDirty = false;
            }
            return customMapsCached;
        }

        /// <summary>
        /// Gets custom map data for a specific map zone
        /// </summary>
        /// <param name="mapZone">The map zone to look for</param>
        /// <returns>The custom map data if found, null otherwise</returns>
        public static CustomMapData GetCustomMap(MapZone mapZone)
        {
            return GetCustomMaps().FirstOrDefault(m => m.MapZone == mapZone);
        }

        /// <summary>
        /// Gets custom map data for a specific custom map zone
        /// </summary>
        /// <param name="customMapZone">The custom map zone to look for</param>
        /// <returns>The custom map data if found, null otherwise</returns>
        public static CustomMapData GetCustomMap(CustomMapZone customMapZone)
        {
            return GetCustomMaps().FirstOrDefault(m => m.mapZone == customMapZone);
        }

        /// <summary>
        /// Gets custom room data for a specific scene name
        /// </summary>
        /// <param name="sceneName">The scene name to look for</param>
        /// <returns>The custom room data if found, null otherwise</returns>
        public static CustomRoomData GetCustomRoom(string sceneName)
        {
            foreach (var map in GetCustomMaps())
            {
                var room = map.GetRoom(sceneName);
                if (room != null)
                {
                    return room;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if a scene is part of any custom map
        /// </summary>
        /// <param name="sceneName">The scene name to check</param>
        /// <returns>True if the scene is part of a custom map</returns>
        public static bool IsCustomMapScene(string sceneName)
        {
            return GetCustomRoom(sceneName) != null;
        }

        /// <summary>
        /// Gets all custom rooms across all custom maps
        /// </summary>
        /// <returns>A list of all custom rooms</returns>
        public static List<CustomRoomData> GetAllCustomRooms()
        {
            var allRooms = new List<CustomRoomData>();
            foreach (var map in GetCustomMaps())
            {
                allRooms.AddRange(map.rooms);
            }
            return allRooms;
        }

        /// <summary>
        /// Gets the map zone for a specific scene name
        /// </summary>
        /// <param name="sceneName">The scene name</param>
        /// <returns>The map zone if the scene is part of a custom map, null otherwise</returns>
        public static MapZone? GetMapZoneForScene(string sceneName)
        {
            var room = GetCustomRoom(sceneName);
            return room?.parentMap?.MapZone;
        }

        /// <summary>
        /// Checks if a map zone has custom map data
        /// </summary>
        /// <param name="mapZone">The map zone to check</param>
        /// <returns>True if there is custom map data for this zone</returns>
        public static bool HasCustomMap(MapZone mapZone)
        {
            return GetCustomMap(mapZone) != null;
        }

        /// <summary>
        /// Gets all scenes that belong to a specific custom map zone
        /// </summary>
        /// <param name="mapZone">The map zone</param>
        /// <returns>A list of scene names</returns>
        public static List<string> GetScenesForMapZone(MapZone mapZone)
        {
            var customMap = GetCustomMap(mapZone);
            if (customMap != null)
            {
                return customMap.rooms.Select(r => r.sceneName).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// Creates a scene-to-map-zone dictionary for all custom maps
        /// This is used to integrate with Hollow Knight's PlayerData.InitMapBools() function
        /// </summary>
        /// <returns>A dictionary mapping scene names to map zones</returns>
        public static Dictionary<string, MapZone> GetCustomSceneToMapZoneMapping()
        {
            var mapping = new Dictionary<string, MapZone>();
            
            foreach (var map in GetCustomMaps())
            {
                foreach (var room in map.rooms)
                {
                    if (!string.IsNullOrEmpty(room.sceneName))
                    {
                        mapping[room.sceneName] = map.MapZone;
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Checks if a custom map should be automatically discovered
        /// </summary>
        /// <param name="mapZone">The map zone</param>
        /// <returns>True if the map should be auto-discovered</returns>
        public static bool ShouldAutoDiscoverMap(MapZone mapZone)
        {
            var customMap = GetCustomMap(mapZone);
            return customMap?.autoDiscoverMap ?? false;
        }

        [OnRegistryLoad]
        static void OnRegistryLoad(Registry r)
        {
            cacheDirty = true;
        }

        [OnRegistryUnload]
        static void OnRegistryUnload(Registry r)
        {
            cacheDirty = true;
        }
    }
}