using System;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Makes sure the graphics settings are configured properly
    /// </summary>
    class VerifyGraphicsSettings : DependencyCheck
    {
        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);

            var AIS = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");
            for (int i = 0; i < AIS.arraySize; i++)
            {
                var element = AIS.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element != null && element.name == "Sprites/Default")
                {
                    AIS.DeleteArrayElementAtIndex(i);
                    graphicsSettings.ApplyModifiedProperties();
                    break;
                }
            }

            graphicsSettings.FindProperty("m_TransparencySortMode").intValue = 3;
            graphicsSettings.FindProperty("m_TransparencySortAxis").vector3Value = Vector3.forward;
            graphicsSettings.ApplyModifiedProperties();

            finishCheck(DependencyCheckResult.Complete);
        }
    }
}
