using UnityEngine;

namespace WeaverCore.Internal
{
    [UnityEditor.InitializeOnLoad]
    internal static class InitializeOnLoad
    {
        static InitializeOnLoad()
        {
            Debug.Log("Initialize On Load");
            ModuleInitializer.Initialize();
        }
    }
}
