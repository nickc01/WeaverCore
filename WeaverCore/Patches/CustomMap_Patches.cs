using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Utilities;
using System.Linq;
using WeaverCore;

public static class CustomMap_Patches
{
    private static Dictionary<MapZone, GameObject> customMapAreaObjects = new Dictionary<MapZone, GameObject>();

    [OnHarmonyPatch]
    static void Patch_Init(HarmonyPatcher patcher)
    {
        // Patch GameMap.SetupMap to handle custom map rooms
        {
            var orig = typeof(GameMap).GetMethod(nameof(GameMap.SetupMap), BindingFlags.Public | BindingFlags.Instance);
            var postfix = typeof(CustomMap_Patches).GetMethod(nameof(SetupMapPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, null, postfix);
        }

        // Patch GameMap.WorldMap to handle custom map areas
        {
            var orig = typeof(GameMap).GetMethod(nameof(GameMap.WorldMap), BindingFlags.Public | BindingFlags.Instance);
            var postfix = typeof(CustomMap_Patches).GetMethod(nameof(WorldMapPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, null, postfix);
        }

        // Patch PlayerData to add custom scene mappings
        {
            var orig = typeof(PlayerData).GetMethod("InitMapBools", BindingFlags.NonPublic | BindingFlags.Instance);
            var postfix = typeof(CustomMap_Patches).GetMethod(nameof(InitMapBoolsPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, null, postfix);
        }

        // Patch PlayerData UpdateGameMap to handle custom scenes
        {
            var orig = typeof(PlayerData).GetMethod("UpdateGameMap", BindingFlags.Public | BindingFlags.Instance);
            var postfix = typeof(CustomMap_Patches).GetMethod(nameof(UpdateGameMapPostfix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, null, postfix);
        }

        {
            var orig = typeof(MapNextAreaDisplay).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(CustomMap_Patches).GetMethod(nameof(NextAreaOnEnablePrefix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, prefix, null);
        }
    }

    static bool NextAreaOnEnablePrefix(MapNextAreaDisplay __instance)
    {
        if (__instance.gameMap == null)
        {
            __instance.gameMap = GameManager.instance.gameMap.GetComponent<GameMap>();
        }
        return true;
    }

    /// <summary>
    /// Postfix for GameMap.SetupMap - Adds custom map rooms to the setup process
    /// </summary>
    static void SetupMapPostfix(GameMap __instance, bool pinsOnly = false)
    {
        try
        {
            var pd = PlayerData.instance;
            if (pd == null) return;

            // Handle custom map rooms
            foreach (var customMap in CustomMapUtilities.GetCustomMaps())
            {
                if (customMap == null || customMap.rooms == null) continue;

                // Get or create the area GameObject for this custom map
                var areaObject = GetOrCreateCustomMapArea(__instance, customMap);
                if (areaObject == null) continue;

                // Process each custom room
                foreach (var room in customMap.rooms)
                {
                    if (room == null || string.IsNullOrEmpty(room.sceneName)) continue;

                    // Check if this room should be visible
                    bool shouldBeVisible = pd.GetVariable<List<string>>("scenesMapped").Contains(room.sceneName) ||
                                         pd.GetBool("mapAllRooms") ||
                                         room.alwaysVisible;

                    if (!shouldBeVisible) continue;

                    // Get or create the room GameObject
                    var roomObject = GetOrCreateCustomRoomObject(areaObject, room);
                    if (roomObject != null && pd.GetBool("hasQuill") && !pinsOnly)
                    {
                        roomObject.SetActive(true);

                        // Set up the sprite renderer if needed
                        var spriteRenderer = roomObject.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null && room.roomSprite != null)
                        {
                            spriteRenderer.sprite = room.roomSprite;
                            spriteRenderer.transform.localScale = room.spriteScale;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            WeaverCore.WeaverLog.LogError($"Error in SetupMapPostfix: {e}");
        }
    }

    /// <summary>
    /// Postfix for GameMap.WorldMap - Handles custom map area visibility
    /// </summary>
    static void WorldMapPostfix(GameMap __instance)
    {
        try
        {
            var pd = PlayerData.instance;
            var gm = GameManager.instance;
            if (pd == null || gm == null) return;

            string currentMapZone = gm.GetCurrentMapZone();

            // Handle custom map areas
            foreach (var customMap in CustomMapUtilities.GetCustomMaps())
            {
                if (customMap == null) continue;

                var mapZoneString = customMap.GetInternalName();
                bool hasCustomMap = HasCustomMap(pd, customMap.MapZone);
                bool shouldShowArea = hasCustomMap || 
                                    (currentMapZone == mapZoneString && pd.GetBool("equippedCharm_2")) ||
                                    customMap.autoDiscoverMap;

                var areaObject = GetOrCreateCustomMapArea(__instance, customMap);
                if (areaObject != null && shouldShowArea)
                {
                    areaObject.SetActive(true);
                }
            }
        }
        catch (Exception e)
        {
            WeaverCore.WeaverLog.LogError($"Error in WorldMapPostfix: {e}");
        }
    }

    /// <summary>
    /// Postfix for PlayerData.InitMapBools - Adds custom scene to map zone mappings
    /// </summary>
    static void InitMapBoolsPostfix(PlayerData __instance)
    {
        try
        {
            var customMapping = CustomMapUtilities.GetCustomSceneToMapZoneMapping();
            foreach (var kvp in customMapping)
            {
                // Add custom scene mappings to the sceneToAreaMappingTable
                var sceneToAreaTable = __instance.GetVariable<Dictionary<string, string>>("sceneToAreaMappingTable");
                if (!sceneToAreaTable.ContainsKey(kvp.Key))
                {
                    string mapBoolName = GetMapBoolName(kvp.Value);
                    if (!string.IsNullOrEmpty(mapBoolName))
                    {
                        sceneToAreaTable[kvp.Key] = mapBoolName;
                    }
                }
            }
        }
        catch (Exception e)
        {
            WeaverCore.WeaverLog.LogError($"Error in InitMapBoolsPostfix: {e}");
        }
    }

    /// <summary>
    /// Postfix for PlayerData.UpdateGameMap - Handles custom scene mapping updates
    /// </summary>
    static void UpdateGameMapPostfix(PlayerData __instance)
    {
        try
        {
            if (!__instance.GetBool("hasQuill")) return;

            // Auto-map custom scenes that have been visited
            var scenesVisited = __instance.GetVariable<List<string>>("scenesVisited");
            var scenesMapped = __instance.GetVariable<List<string>>("scenesMapped");
            
            foreach (var sceneName in scenesVisited)
            {
                if (scenesMapped.Contains(sceneName)) continue;

                var customRoom = CustomMapUtilities.GetCustomRoom(sceneName);
                if (customRoom != null && customRoom.autoMap)
                {
                    var customMap = customRoom.parentMap;
                    if (customMap != null && HasMapForCustomScene(customMap, sceneName))
                    {
                        scenesMapped.Add(sceneName);
                    }
                }
            }
        }
        catch (Exception e)
        {
            WeaverCore.WeaverLog.LogError($"Error in UpdateGameMapPostfix: {e}");
        }
    }

    /// <summary>
    /// Gets or creates a GameObject for a custom map area
    /// </summary>
    private static GameObject GetOrCreateCustomMapArea(GameMap gameMap, CustomMapData customMap)
    {
        if (customMapAreaObjects.TryGetValue(customMap.MapZone, out var existingArea))
        {
            return existingArea;
        }

        // Create new area GameObject
        var areaObject = new GameObject($"area{customMap.GetInternalName()}");
        areaObject.transform.SetParent(gameMap.transform);
        areaObject.transform.localPosition = Vector3.zero;
        areaObject.SetActive(false);

        customMapAreaObjects[customMap.MapZone] = areaObject;

        return areaObject;
    }

    /// <summary>
    /// Gets or creates a GameObject for a custom room
    /// </summary>
    private static GameObject GetOrCreateCustomRoomObject(GameObject areaObject, CustomRoomData room)
    {
        // Check if room object already exists
        var existingRoom = areaObject.transform.Find(room.sceneName);
        if (existingRoom != null)
        {
            return existingRoom.gameObject;
        }

        // Create new room GameObject
        var roomObject = new GameObject(room.sceneName);
        roomObject.transform.SetParent(areaObject.transform);
        roomObject.transform.localPosition = new Vector3(room.mapPosition.x, room.mapPosition.y, 0);
        roomObject.SetActive(false);

        // Add SpriteRenderer if we have a sprite
        if (room.roomSprite != null)
        {
            var spriteRenderer = roomObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = room.roomSprite;
            spriteRenderer.transform.localScale = room.spriteScale;
            
            // Set proper sorting layer and order
            spriteRenderer.sortingLayerName = "HUD";
            spriteRenderer.sortingOrder = 0;
        }

        return roomObject;
    }

    /// <summary>
    /// Checks if the player has the map for a custom map zone
    /// </summary>
    private static bool HasCustomMap(PlayerData pd, MapZone mapZone)
    {
        string mapBoolName = GetMapBoolName(mapZone);
        if (string.IsNullOrEmpty(mapBoolName))
        {
            // If no bool name, check if it should be auto-discovered
            var customMap = CustomMapUtilities.GetCustomMap(mapZone);
            return customMap?.autoDiscoverMap ?? false;
        }

        try
        {
            return pd.GetBool(mapBoolName);
        }
        catch
        {
            // If the field doesn't exist, check if it should be auto-discovered
            var customMap = CustomMapUtilities.GetCustomMap(mapZone);
            return customMap?.autoDiscoverMap ?? false;
        }
    }

    /// <summary>
    /// Checks if the player has the map that contains this custom scene
    /// </summary>
    private static bool HasMapForCustomScene(CustomMapData customMap, string sceneName)
    {
        if (customMap.autoDiscoverMap)
            return true;

        var pd = PlayerData.instance;
        if (pd == null)
            return false;

        return HasCustomMap(pd, customMap.MapZone);
    }

    /// <summary>
    /// Gets the PlayerData boolean field name for a map zone
    /// </summary>
    private static string GetMapBoolName(MapZone mapZone)
    {
        // Handle built-in map zones
        switch (mapZone)
        {
            case MapZone.CROSSROADS: return "mapCrossroads";
            case MapZone.GREEN_PATH: return "mapGreenpath";
            case MapZone.FOG_CANYON: return "mapFogCanyon";
            case MapZone.ROYAL_GARDENS: return "mapRoyalGardens";
            case MapZone.WASTES: return "mapFungalWastes";
            case MapZone.CITY: return "mapCity";
            case MapZone.WATERWAYS: return "mapWaterways";
            case MapZone.MINES: return "mapMines";
            case MapZone.DEEPNEST: return "mapDeepnest";
            case MapZone.CLIFFS: return "mapCliffs";
            case MapZone.OUTSKIRTS: return "mapOutskirts";
            case MapZone.RESTING_GROUNDS: return "mapRestingGrounds";
            case MapZone.ABYSS: return "mapAbyss";
            default:
                // For custom map zones, generate a field name
                if (MapZoneUtilities.IsCustomMapZone(mapZone))
                {
                    var customZone = MapZoneUtilities.GetCustomMapZones().FirstOrDefault(z => z.MapZone == mapZone);
                    if (customZone != null)
                    {
                        return $"map{customZone.GetInternalName()}";
                    }
                }
                return null;
        }
    }
}