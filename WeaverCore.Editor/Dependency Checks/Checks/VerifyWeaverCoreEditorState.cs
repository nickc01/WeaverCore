using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Verifies the WeaverCore.Editor asmdef is setup properly for development
    /// </summary>
    class VerifyWeaverCoreEditorState : DependencyCheck
    {
        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            var projectInfo = ScriptFinder.GetProjectScriptInfo();

            var asm = projectInfo.FirstOrDefault(p => p.AssemblyName.Contains("WeaverCore.Editor"));

            if (asm == null)
            {
                throw new Exception("Unable to find assembly \"WeaverCore.Editor\". Your WeaverCore files may not be in a valid state");
            }

            if (asm.Definition.IncludePlatforms.Count > 0)
            {
                asm.Definition.ExcludePlatforms = new System.Collections.Generic.List<AssemblyDefinitionFile.Platform>();
                asm.Definition.IncludePlatforms = new System.Collections.Generic.List<AssemblyDefinitionFile.Platform>();

                asm.Save();

                EditorDebugUtilities.ClearLog();

                finishCheck(DependencyCheckResult.RequiresReload);

                AssetDatabase.ImportAsset(asm.AssemblyDefinitionPath, ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
            else
            {
                finishCheck(DependencyCheckResult.Complete);
            }
        }
    }
}
