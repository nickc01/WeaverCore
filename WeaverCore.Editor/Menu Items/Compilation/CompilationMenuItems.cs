using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
	public static class CompilationMenuItems
	{
		[MenuItem("WeaverCore/Compilation/Mod %F5")]
		public static void CompileMod()
		{
			BuildScreen.ShowBuildScreen(false);
		}

		[MenuItem("WeaverCore/Compilation/WeaverCore")]
		public static void CompileWeaverCore()
		{
			BuildScreen.ShowBuildScreen(true);
		}

        [MenuItem("WeaverCore/Compilation/WeaverCore.Game")]
        public static void CompileWeaverCoreGame()
        {
            BuildTools.BuildPartialWeaverCore(BuildTools.WeaverCoreBuildLocation);
        }

        [MenuItem("WeaverCore/Compilation/Quick Compile Mod %F6")]
        public static void QuickCompileMod()
        {
            try
            {
                if (!BuildTools.BuildLocationSet())
                {
                    return;
                }
                var outputPath = new FileInfo(BuildTools.GetModBuildFileLocation());
                QuickCompileSystem.QuickCompileMod(outputPath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Quick Compile failed: {e.Message}");
            }
        }

        [MenuItem("WeaverCore/Compilation/Quick Compile Mod %F6", validate = true)]
        public static bool ValidateQuickCompileMod()
        {
            try
            {
                if (!BuildTools.BuildLocationSet())
                {
                    return false;
                }
                var outputPath = new FileInfo(BuildTools.GetModBuildFileLocation());
                return QuickCompileSystem.CanQuickCompile(outputPath);
            }
            catch
            {
                return false;
            }
        }

        [MenuItem("WeaverCore/Compilation/Quick Compile WeaverCore")]
        public static void QuickCompileWeaverCore()
        {
            try
            {
                if (!BuildTools.BuildLocationSet())
                {
                    return;
                }
                var outputPath = new FileInfo(BuildTools.GetModBuildFolder() + "WeaverCore/WeaverCore.dll");
                QuickCompileSystem.QuickCompileWeaverCore(outputPath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Quick Compile WeaverCore failed: {e.Message}");
            }
        }

        [MenuItem("WeaverCore/Compilation/Quick Compile WeaverCore", validate = true)]
        public static bool ValidateQuickCompileWeaverCore()
        {
            try
            {
                if (!BuildTools.BuildLocationSet())
                {
                    return false;
                }
                var modBuildFolder = BuildTools.GetModBuildFolder();
                var outputPath = new FileInfo(modBuildFolder + "WeaverCore/WeaverCore.dll");
                return QuickCompileSystem.CanQuickCompileWeaverCore(outputPath);
            }
            catch
            {
                return false;
            }
        }
    }
}
