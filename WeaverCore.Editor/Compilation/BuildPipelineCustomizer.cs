using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Build.Content;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
    public abstract class BuildPipelineCustomizer
    {
        /// <summary>
        /// Used for adding any extra properties to the Build Screen
        /// </summary>
        public virtual void BuildScreenOnGUIExtension(BuildScreen sourceScreen)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build Location");
            EditorGUILayout.BeginHorizontal();
            //BuildSettings.BuildLocation = EditorGUILayout.TextField(BuildSettings.BuildLocation);
        }

        /// <summary>
        /// Used to verify the build settings on the build screen before starting the build
        /// </summary>
        public virtual bool PreBuildVerify()
        {
            return true;
        }
    }
}
