#if UNITY_EDITOR
#endif

using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class GameObjectUtilities
    {
        public static void ActivateAllChildren(this GameObject gm, bool active)
        {
            gm.SetActive(active);
            for (int i = 0; i < gm.transform.childCount; i++)
            {
                ActivateAllChildren(gm.transform.GetChild(i).gameObject, active);
            }
        }
    }
}