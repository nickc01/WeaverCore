using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
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

            if (asm.Definition.includePlatforms.Count > 0)
            {
                asm.Definition.excludePlatforms = new List<string>();
                asm.Definition.includePlatforms = new List<string>();
                //Debug.Log("Asm Definition Path = " + asm.AssemblyDefinitionPath);
                //Debug.Log("Importing Asset = " + asm.AssemblyDefinitionPath);
                asm.Save();

                DebugUtilities.ClearLog();

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
