using System;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Verifies the .Net API level of the project is set to 4.6
    /// </summary>
    class VerifyAPILevel : DependencyCheck
    {
        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) != ApiCompatibilityLevel.NET_4_6)
            {
                DebugUtilities.ClearLog();
                Debug.Log("Updating Project API Level from .Net Standard 2.0 to .Net 4.6");
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
                finishCheck(DependencyCheckResult.RequiresReload);
            }
            else
            {
                finishCheck(DependencyCheckResult.Complete);
            }
            finishCheck(DependencyCheckResult.Complete);
        }
    }
}
