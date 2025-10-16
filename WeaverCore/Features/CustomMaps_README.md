# WeaverCore Custom Maps System

This system allows you to create custom map zones with individual room sprites that integrate seamlessly with Hollow Knight's map system.

## Overview

The custom maps system consists of several components:

- **CustomMapZone**: Defines a custom map zone (already existed)
- **CustomMapData**: Defines the layout, background, and rooms for a custom map
- **CustomRoomData**: Defines individual room sprites and their positions (embedded in CustomMapData)
- **CustomMapManager**: Component that handles map integration in your scenes
- **CustomMapUtilities**: Utility functions for managing custom maps

## Setup Guide

### Step 1: Create a Custom Map Zone

1. Right-click in Project window → Create → WeaverCore → Custom Map Zone
2. Set the Map Zone Name and a unique Map Zone ID
3. Optionally set a background image for the save file selection menu

### Step 2: Create Custom Map Data

1. Right-click in Project window → Create → WeaverCore → Custom Map Data
2. Assign your CustomMapZone to the "Custom Map Zone" field
3. Configure the map background sprite, offset, and scale
4. Set the map bounds (the coordinate space for positioning rooms)

### Step 3: Add Rooms to Your Map

1. In the CustomMapData inspector, click "Add Room" 
2. For each room, configure:
   - **Scene Name**: The exact name of the Unity scene
   - **Display Name**: Optional friendly name for the room
   - **Room Sprite**: The sprite that appears on the map for this room
   - **Map Position**: Where the room appears within the map bounds
   - **Sprite Scale**: How large the room sprite appears
   - **Auto Map**: Whether the room is automatically mapped when visited (requires quill)
   - **Always Visible**: Whether the room is always visible on the map
   - **Room Type**: Normal, Entrance, Shop, Boss, Special, or Transit

### Step 4: Add CustomMapManager to Your Scenes

1. In each scene that's part of your custom map, add the CustomMapManager component to any GameObject
2. Enable "Auto Find Room Data" to automatically configure based on scene name
3. Alternatively, manually assign the CustomMapData and specific CustomRoomData

### Step 5: Register Your Assets

1. Add your CustomMapZone and CustomMapData to a WeaverCore Registry
2. This ensures they're loaded and integrated with the map system

## Map Discovery Options

### Automatic Discovery
Set `autoDiscoverMap = true` in CustomMapData to make the map automatically appear when the player enters the zone.

### Cornifer Integration
- Set `autoDiscoverMap = false`
- Set `corniferLocation` to the scene where Cornifer should sell the map
- Set `mapCost` for the purchase price
- The system will handle the rest automatically

## Advanced Usage

### Custom Map Bounds
The `mapBounds` field defines the coordinate system for positioning rooms. This allows you to:
- Use any coordinate system you prefer
- Position rooms relative to a logical map layout
- Scale and offset the entire map as needed

### Room Types
Different room types can be used for visual distinction:
- **Normal**: Standard rooms
- **Entrance**: Entry points to the area
- **Shop**: Commercial locations
- **Boss**: Boss battle rooms
- **Special**: Unique locations
- **Transit**: Connection/travel rooms

### Runtime Map Management
Use CustomMapUtilities to:
- Get custom map data: `CustomMapUtilities.GetCustomMap(mapZone)`
- Check if a scene is custom: `CustomMapUtilities.IsCustomMapScene(sceneName)`
- Get all custom rooms: `CustomMapUtilities.GetAllCustomRooms()`

### Programmatic Map Control
The CustomMapManager component provides methods to:
- Force map a room: `manager.ForceMapRoom()`
- Check map discovery: `manager.IsCustomMapDiscovered()`
- Set map discovered: `manager.SetCustomMapDiscovered(true)`

## Integration Details

The system integrates with Hollow Knight's existing map system by:

1. **Patching GameMap.SetupMap()**: Adds custom rooms to the map rendering process
2. **Patching GameMap.WorldMap()**: Handles custom map area visibility
3. **Patching PlayerData.InitMapBools()**: Adds custom scene-to-area mappings
4. **Patching PlayerData.UpdateGameMap()**: Handles automatic mapping of custom scenes

## Troubleshooting

### Map Not Appearing
- Ensure CustomMapZone and CustomMapData are in a Registry
- Check that the scene names in CustomRoomData match your actual scene names
- Verify the CustomMapManager is in your scenes

### Rooms Not Mapping
- Ensure player has the quill (`pd.hasQuill`)
- Check that `autoMap` is enabled for the room
- Verify the map has been discovered or `autoDiscoverMap` is enabled

### Sprites Not Showing
- Ensure room sprites are assigned in CustomRoomData
- Check that sprite scale is appropriate (not zero or too small)
- Verify map bounds contain the room positions

## Example Workflow

1. Create CustomMapZone "My Custom Area" with ID 1000
2. Create CustomMapData and assign the zone
3. Add rooms for scenes "MyArea_01", "MyArea_02", "MyArea_Boss"
4. Set appropriate sprites and positions for each room
5. Add CustomMapManager components to each scene
6. Add everything to a Registry
7. Test in-game by visiting the scenes

The system will automatically handle the rest!