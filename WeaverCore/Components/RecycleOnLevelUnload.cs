using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Component that recycles the GameObject when the level is unloaded.
    /// </summary>
    public class RecycleOnLevelUnload : MonoBehaviour
    {
        private bool ApplicationIsQuitting;

        /// <summary>
        /// Called when the component becomes enabled and active.
        /// Subscribes to the DestroyPersonalPools event in GameManager.
        /// </summary>
        private void OnEnable()
        {
            GameManager.instance.DestroyPersonalPools += RecycleSelf;
        }

        /// <summary>
        /// Called when the component becomes disabled.
        /// Unsubscribes from the DestroyPersonalPools event in GameManager,
        /// unless the application is quitting.
        /// </summary>
        private void OnDisable()
        {
            if (!ApplicationIsQuitting)
            {
                GameManager.instance.DestroyPersonalPools -= RecycleSelf;
            }
        }

        /// <summary>
        /// Called when the application is about to quit.
        /// Sets the ApplicationIsQuitting flag to true.
        /// </summary>
        private void OnApplicationQuit()
        {
            ApplicationIsQuitting = true;
        }

        /// <summary>
        /// Recycles the GameObject by destroying it.
        /// </summary>
        private void RecycleSelf()
        {
            GameObject.Destroy(gameObject);
        }
    }
}
