using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;

namespace WeaverCore.Utilities
{
    public static class GlobalPoolScenePositionFixer
    {
        const string TargetObjectName = "GlobalPool";
        const float TargetX = -999999f;
        const float TargetY = -999999f;

        [OnRuntimeInit]
        static void OnRuntimeInit()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (GlobalPrefabDefaults.Instance != null)
            {
                var currentPos = GlobalPrefabDefaults.Instance.transform.position;
                GlobalPrefabDefaults.Instance.transform.position = new Vector3(TargetX, TargetY, currentPos.z);
            }
        }
    }
}
