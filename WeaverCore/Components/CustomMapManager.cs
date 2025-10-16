using GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Manages custom maps and handles their integration with the game's map system.
    /// This component should be added to scenes that are part of custom maps.
    /// </summary>
    public class CustomMapManager : MonoBehaviour
    {
        [Header("Custom Map Settings")]
        [Tooltip("The custom map data this scene belongs to")]
        public CustomMapData customMapData;
        
        [Tooltip("The specific room data for this scene")]
        public CustomRoomData roomData;

        [Header("Auto-Configuration")]
        [Tooltip("Automatically find room data based on the current scene name")]
        public bool autoFindRoomData = true;

        [Tooltip("Override the scene name used for mapping")]
        public string sceneNameOverride;

        private static List<CustomMapManager> activeManagers = new List<CustomMapManager>();
        
        private void Awake()
        {
            activeManagers.Add(this);
            
            if (autoFindRoomData)
            {
                AutoConfigureRoomData();
            }
        }

        private void Start()
        {
            InitializeCustomMap();
        }

        private void OnDestroy()
        {
            activeManagers.Remove(this);
        }

        /// <summary>
        /// Automatically finds and configures room data based on the scene name
        /// </summary>
        private void AutoConfigureRoomData()
        {
            string sceneName = GetEffectiveSceneName();
            
            // Find room data for this scene
            roomData = CustomMapUtilities.GetCustomRoom(sceneName);
            
            // If we found room data, also get the parent map data
            if (roomData != null && customMapData == null)
            {
                customMapData = roomData.parentMap;
            }
        }

        /// <summary>
        /// Initializes the custom map for this scene
        /// </summary>
        private void InitializeCustomMap()
        {
            if (customMapData == null || roomData == null)
            {
                return;
            }

            var pd = PlayerData.instance;
            var gm = GameManager.instance;
            
            if (pd == null || gm == null)
            {
                return;
            }

            string sceneName = GetEffectiveSceneName();

            // Set up scene-to-map-zone mapping if needed
            var sceneToAreaTable = pd.GetVariable<Dictionary<string, string>>("sceneToAreaMappingTable");
            if (!sceneToAreaTable.ContainsKey(sceneName))
            {
                string mapBoolName = $"map{customMapData.GetInternalName()}";
                sceneToAreaTable[sceneName] = mapBoolName;
            }

            // Auto-discover map if configured to do so
            if (customMapData.autoDiscoverMap)
            {
                SetCustomMapDiscovered(true);
            }

            // Mark scene as visited
            var scenesVisited = pd.GetVariable<List<string>>("scenesVisited");
            if (!scenesVisited.Contains(sceneName))
            {
                scenesVisited.Add(sceneName);
            }

            // Auto-map this room if it should be auto-mapped and player has quill
            if (roomData.autoMap && pd.GetBool("hasQuill"))
            {
                var scenesMapped = pd.GetVariable<List<string>>("scenesMapped");
                if (!scenesMapped.Contains(sceneName) && HasMapForScene(sceneName))
                {
                    scenesMapped.Add(sceneName);
                }
            }
        }

        /// <summary>
        /// Gets the effective scene name for mapping purposes
        /// </summary>
        private string GetEffectiveSceneName()
        {
            if (!string.IsNullOrEmpty(sceneNameOverride))
            {
                return sceneNameOverride;
            }

            // Try to get scene name from GameManager
            var gm = GameManager.instance;
            if (gm != null && !string.IsNullOrEmpty(gm.GetSceneNameString()))
            {
                return gm.GetSceneNameString();
            }

            // Fall back to Unity scene name
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Checks if the player has the map that contains this scene
        /// </summary>
        private bool HasMapForScene(string sceneName)
        {
            if (customMapData == null)
                return false;

            if (customMapData.autoDiscoverMap)
                return true;

            var pd = PlayerData.instance;
            if (pd == null)
                return false;

            // Check if custom map is discovered
            return IsCustomMapDiscovered();
        }

        /// <summary>
        /// Sets whether the custom map has been discovered/purchased
        /// </summary>
        public void SetCustomMapDiscovered(bool discovered)
        {
            if (customMapData == null)
                return;

            var pd = PlayerData.instance;
            if (pd == null)
                return;

            string mapBoolName = $"map{customMapData.GetInternalName()}";
            
            try
            {
                pd.SetBool(mapBoolName, discovered);
            }
            catch (System.Exception e)
            {
                // If the field doesn't exist in PlayerData, we can still mark rooms as mapped
                // This allows custom maps to work even without modifying PlayerData
                if (discovered)
                {
                    var scenesMapped = pd.GetVariable<List<string>>("scenesMapped");
                    foreach (var room in customMapData.rooms)
                    {
                        if (!scenesMapped.Contains(room.sceneName))
                        {
                            scenesMapped.Add(room.sceneName);
                        }
                    }
                }
                WeaverCore.WeaverLog.LogWarning($"Custom map field '{mapBoolName}' doesn't exist in PlayerData, using fallback mapping: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if the custom map has been discovered/purchased
        /// </summary>
        public bool IsCustomMapDiscovered()
        {
            if (customMapData == null)
                return false;

            if (customMapData.autoDiscoverMap)
                return true;

            var pd = PlayerData.instance;
            if (pd == null)
                return false;

            string mapBoolName = $"map{customMapData.GetInternalName()}";
            
            try
            {
                return pd.GetBool(mapBoolName);
            }
            catch
            {
                // If the field doesn't exist, check if the rooms are mapped
                var scenesMapped = pd.GetVariable<List<string>>("scenesMapped");
                return customMapData.rooms.Any(room => scenesMapped.Contains(room.sceneName));
            }
        }

        /// <summary>
        /// Forces this room to appear on the map
        /// </summary>
        public void ForceMapRoom()
        {
            if (roomData == null)
                return;

            var pd = PlayerData.instance;
            if (pd == null)
                return;

            string sceneName = GetEffectiveSceneName();
            var scenesMapped = pd.GetVariable<List<string>>("scenesMapped");
            if (!scenesMapped.Contains(sceneName))
            {
                scenesMapped.Add(sceneName);
            }
        }

        /// <summary>
        /// Gets all active custom map managers in the scene
        /// </summary>
        public static List<CustomMapManager> GetActiveManagers()
        {
            return new List<CustomMapManager>(activeManagers);
        }

        /// <summary>
        /// Gets the custom map manager for a specific scene name
        /// </summary>
        public static CustomMapManager GetManagerForScene(string sceneName)
        {
            return activeManagers.FirstOrDefault(m => m.GetEffectiveSceneName() == sceneName);
        }

        /// <summary>
        /// Called when the player purchases/discovers a map from Cornifer
        /// </summary>
        [OnInit]
        public static void Initialize()
        {
            // Hook into any Cornifer interactions if needed
            // This could be extended to handle map purchasing
        }

        #region Unity Editor Helpers
        
#if UNITY_EDITOR
        [ContextMenu("Auto-Configure Room Data")]
        private void EditorAutoConfigureRoomData()
        {
            AutoConfigureRoomData();
        }

        [ContextMenu("Force Map This Room")]
        private void EditorForceMapRoom()
        {
            ForceMapRoom();
        }

        private void OnValidate()
        {
            if (autoFindRoomData && Application.isPlaying)
            {
                AutoConfigureRoomData();
            }
        }
#endif

        #endregion
    }
}