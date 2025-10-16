using GlobalEnums;
using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
    /// <summary>
    /// Used to define custom map layouts, backgrounds, and room positions for custom map zones
    /// </summary>
    [ShowFeature]
    [CreateAssetMenu(fileName = "Custom Map Data", menuName = "WeaverCore/Custom Map Data")]
    public class CustomMapData : ScriptableObject
    {
        [Header("Map Zone Reference")]
        /// <summary>
        /// The custom map zone this map data belongs to
        /// </summary>
        [Tooltip("The custom map zone this map data belongs to")]
        public CustomMapZone mapZone;

        [Header("Map Visual Settings")]
        /// <summary>
        /// The background sprite used for the map when opened
        /// </summary>
        [Tooltip("The background sprite used for the map when opened")]
        public Sprite mapBackground;

        /// <summary>
        /// The position offset for the map background
        /// </summary>
        [Tooltip("The position offset for the map background")]
        public Vector2 backgroundOffset = Vector2.zero;

        /// <summary>
        /// The scale for the map background
        /// </summary>
        [Tooltip("The scale for the map background")]
        public Vector2 backgroundScale = Vector2.one;

        [Header("Room Layout")]
        /// <summary>
        /// List of custom rooms that belong to this map
        /// </summary>
        [Tooltip("List of custom rooms that belong to this map")]
        public List<CustomRoomData> rooms = new List<CustomRoomData>();

        [Header("Map Discovery Settings")]
        /// <summary>
        /// Whether this map should be automatically discovered when the zone is entered
        /// </summary>
        [Tooltip("Whether this map should be automatically discovered when the zone is entered")]
        public bool autoDiscoverMap = false;

        /// <summary>
        /// The scene name where Cornifer appears to sell this map (optional)
        /// </summary>
        [Tooltip("The scene name where Cornifer appears to sell this map (optional)")]
        public string corniferLocation = "";

        /// <summary>
        /// The cost to purchase this map from Cornifer
        /// </summary>
        [Tooltip("The cost to purchase this map from Cornifer")]
        public int mapCost = 60;

        [Header("Map Bounds")]
        /// <summary>
        /// The bounds of this custom map for positioning rooms
        /// </summary>
        [Tooltip("The bounds of this custom map for positioning rooms")]
        public Rect mapBounds = new Rect(0, 0, 100, 100);

        /// <summary>
        /// Gets the map zone enum value
        /// </summary>
        public MapZone MapZone => mapZone != null ? mapZone.MapZone : MapZone.NONE;

        /// <summary>
        /// Gets the internal name for this map
        /// </summary>
        public string GetInternalName()
        {
            return mapZone != null ? mapZone.GetInternalName() : "UNKNOWN_MAP";
        }

        /// <summary>
        /// Adds a room to this map
        /// </summary>
        /// <param name="room">The room data to add</param>
        public void AddRoom(CustomRoomData room)
        {
            if (!rooms.Contains(room))
            {
                rooms.Add(room);
                room.parentMap = this;
            }
        }

        /// <summary>
        /// Removes a room from this map
        /// </summary>
        /// <param name="room">The room data to remove</param>
        public void RemoveRoom(CustomRoomData room)
        {
            if (rooms.Contains(room))
            {
                rooms.Remove(room);
                if (room.parentMap == this)
                {
                    room.parentMap = null;
                }
            }
        }

        /// <summary>
        /// Gets a room by scene name
        /// </summary>
        /// <param name="sceneName">The name of the scene</param>
        /// <returns>The room data if found, null otherwise</returns>
        public CustomRoomData GetRoom(string sceneName)
        {
            return rooms.Find(r => r.sceneName == sceneName);
        }

        /// <summary>
        /// Checks if this map contains a room with the given scene name
        /// </summary>
        /// <param name="sceneName">The name of the scene</param>
        /// <returns>True if the room exists in this map</returns>
        public bool ContainsRoom(string sceneName)
        {
            return GetRoom(sceneName) != null;
        }

        private void OnValidate()
        {
            // Ensure all rooms reference this map as their parent
            foreach (var room in rooms)
            {
                if (room != null && room.parentMap != this)
                {
                    room.parentMap = this;
                }
            }
        }
    }

    /// <summary>
    /// Data structure for individual room information within a custom map
    /// </summary>
    [System.Serializable]
    public class CustomRoomData
    {
        [Header("Scene Information")]
        /// <summary>
        /// The name of the scene this room represents
        /// </summary>
        [Tooltip("The name of the scene this room represents")]
        public string sceneName;

        /// <summary>
        /// Display name for the room (optional, uses scene name if not provided)
        /// </summary>
        [Tooltip("Display name for the room (optional, uses scene name if not provided)")]
        public string displayName;

        [Header("Map Sprite")]
        /// <summary>
        /// The sprite shown on the map when this room is discovered
        /// </summary>
        [Tooltip("The sprite shown on the map when this room is discovered")]
        public Sprite roomSprite;

        /// <summary>
        /// The position of this room on the map relative to the map bounds
        /// </summary>
        [Tooltip("The position of this room on the map relative to the map bounds")]
        public Vector2 mapPosition;

        /// <summary>
        /// The scale of the room sprite on the map
        /// </summary>
        [Tooltip("The scale of the room sprite on the map")]
        public Vector2 spriteScale = Vector2.one;

        [Header("Room Properties")]
        /// <summary>
        /// Whether this room should be automatically mapped when visited (requires quill)
        /// </summary>
        [Tooltip("Whether this room should be automatically mapped when visited (requires quill)")]
        public bool autoMap = true;

        /// <summary>
        /// Whether this room is always visible on the map (even without mapping)
        /// </summary>
        [Tooltip("Whether this room is always visible on the map (even without mapping)")]
        public bool alwaysVisible = false;

        /// <summary>
        /// Special room type (entrance, shop, boss, etc.)
        /// </summary>
        [Tooltip("Special room type for different visual treatment")]
        public RoomType roomType = RoomType.Normal;

        [Header("Internal References")]
        /// <summary>
        /// The parent map this room belongs to
        /// </summary>
        [HideInInspector]
        public CustomMapData parentMap;

        /// <summary>
        /// Gets the display name, falling back to scene name if not set
        /// </summary>
        public string GetDisplayName()
        {
            return !string.IsNullOrEmpty(displayName) ? displayName : sceneName;
        }
    }

    /// <summary>
    /// Enum for different types of rooms that may need special visual treatment
    /// </summary>
    public enum RoomType
    {
        Normal,
        Entrance,
        Shop,
        Boss,
        Special,
        Transit
    }
}