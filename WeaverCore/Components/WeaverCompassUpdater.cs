using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverCompassUpdater : MonoBehaviour
    {
        bool hooked = false;

        private void Awake()
        {
            if (enabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (!hooked)
            {
                hooked = true;
                EventManager.OnEventTriggered += EventManager_OnEventTriggered;
            }
        }

        private void OnDisable()
        {
            if (hooked)
            {
                hooked = false;
                EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
            }
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            if (eventName == "SET COMPASS POINT" && destination == gameObject)
            {
                UpdateCompassPosition();
            }
        }

        /// <summary>
        /// Updates the compass point on the map to whereever the player is currently located
        /// </summary>
        public void UpdateCompassPosition() => UpdateCompassPosition(Player.Player1.transform.position);

        /// <summary>
        /// Updates the compass point on the map to the specified position
        /// </summary>
        /// <param name="worldPosition">The position in the world the compass should point towards</param>
        public void UpdateCompassPosition(Vector2 worldPosition)
        {
            var sceneName = GameManager.instance.GetSceneNameString();

            var mapZone = GameManager.instance.GetCurrentMapZone();

            var gameMapType = typeof(GameManager).Assembly.GetType("GameMap");

            if (gameMapType != null)
            {
                var gameMap = GameObject.FindObjectOfType(gameMapType) as MonoBehaviour;

                gameMap.ReflectCallMethod("SetDoorValues", new object[] { worldPosition.x, worldPosition.y, sceneName, mapZone });

                PlayerData.instance.SetFloat("gMap_doorX", worldPosition.x);
                PlayerData.instance.SetFloat("gMap_doorY", worldPosition.y);
                PlayerData.instance.SetString("gMap_doorScene", sceneName);
                PlayerData.instance.SetString("gMap_doorMapZone", mapZone);

                gameMap.ReflectCallMethod("SetCompassPoint");
            }
        }
    }
}
