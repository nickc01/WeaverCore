using UnityEngine;

namespace WeaverCore.Components
{
    public class WeaverCompassUpdater : MonoBehaviour
    {
        [SerializeField]
        bool updateOnAwake = true;

        private void Awake()
        {
            EventManager.OnEventTriggered += EventManager_OnEventTriggered;
            /*if (updateOnAwake)
            {
                HeroController.instance.heroInPosition += Instance_heroInPosition;
            }*/
        }

        private void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            //WeaverLog.Log("EVENT = " + eventName);
        }

        /*private void Instance_heroInPosition(bool forceDirect)
        {
            UpdateCompassPosition();
            HeroController.instance.heroInPosition -= Instance_heroInPosition;
        }*/

        public void UpdateCompassPosition()
        {
            
        }
    }
}
