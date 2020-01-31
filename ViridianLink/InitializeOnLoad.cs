using UnityEngine;

namespace ViridianLink
{
	[UnityEditor.InitializeOnLoad]
    static class InitializeOnLoad
    {
        static InitializeOnLoad()
        {
            Debug.Log("Initialize On Load");
            ModuleInitializer.Initialize();
        }
    }
}
