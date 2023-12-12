using UnityEngine;

namespace WeaverCore.Components
{
    public class RecycleOnLevelUnload : MonoBehaviour
    {
        private bool ApplicationIsQuitting;
        private void OnEnable()
        {
            GameManager.instance.DestroyPersonalPools += RecycleSelf;
        }

        private void OnDisable()
        {
            if (!ApplicationIsQuitting)
            {
                GameManager.instance.DestroyPersonalPools -= RecycleSelf;
            }
        }

        private void OnApplicationQuit()
        {
            ApplicationIsQuitting = true;
        }

        private void RecycleSelf()
        {
            GameObject.Destroy(gameObject);
        }
    }
}
