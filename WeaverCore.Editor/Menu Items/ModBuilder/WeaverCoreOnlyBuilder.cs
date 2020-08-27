using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Systems;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class WeaverCoreOnlyBuilder
	{
		[MenuItem("WeaverCore/Compile WeaverCore")]
		public static void Compile()
		{
			//PersistenceManager.AddFunctionToCall(typeof(WeaverCoreOnlyBuilder).GetMethod("PersistenceTest", BindingFlags.Static | BindingFlags.NonPublic));
			/*foreach (var file in MonoScriptUtilities.GetScriptPathsUnderAssembly("HollowKnight"))
			{
				Debug.Log("Hollow Knight File = " + file.Name);
			}
			LibraryCompiler.ChangeAssemblyNames();*/








			//UnboundCoroutine.Start(BeginCompile());
			var preCompAssembliesF = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilation").GetField("precompiledAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
			var unityAssembliesF = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilation").GetField("unityAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
			var targetAssembliesF = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilation").GetMethod("GetTargetAssemblies", BindingFlags.Public | BindingFlags.Instance);
			var instance = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface").GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);

			//Debug.Log("Type = " + preCompAssemblies.GetValue(instance));
			var preCompAssemblies = (Array)preCompAssembliesF.GetValue(instance);
			var unityAssemblies = (Array)unityAssembliesF.GetValue(instance);
			var targetAssemblies = (Array)targetAssembliesF.Invoke(instance, null);

			foreach (var preComp in preCompAssemblies)
			{
				var path = (string)preComp.GetType().GetField("Path").GetValue(preComp);
				Debug.Log("PreCompilation Assembly = " + path);
			}

			foreach (var unityAsm in unityAssemblies)
			{
				var path = (string)unityAsm.GetType().GetField("Path").GetValue(unityAsm);
				Debug.Log("Unity Assembly = " + path);
			}

			foreach (var targetAssembly in targetAssemblies)
			{
				var path = (string)targetAssembly.GetType().GetField("Name").GetValue(targetAssembly);
				Debug.Log("Target Assembly = " + path);
			}
		}


		static void PersistenceTest()
		{
			Debug.Log("This function was called after a domain reload!!!");
		}

		static IEnumerator BeginCompile()
		{
			var settings = new Settings();
			settings.GetStoredSettings();
			//WeaverCoreBuilder.BuildFinishedVersion(settings.HollowKnightDirectory + "\\hollow_knight_data\\Managed\\Mods\\WeaverCore.dll");
			LibraryCompiler.BuildWeaverCore(settings.HollowKnightDirectory + "\\hollow_knight_data\\Managed\\Mods\\WeaverCore.dll");


			List<BundleBuild> bundles = new List<BundleBuild>();

			yield return LibraryCompiler.BuildWeaverCoreBundles(bundles, LibraryCompiler.MainTargets);

			foreach (var bundle in bundles)
			{
				EmbedResourceCMD.EmbedResource(settings.HollowKnightDirectory + "\\hollow_knight_data\\Managed\\Mods\\WeaverCore.dll", bundle.File.FullName, bundle.File.Name + PlatformUtilities.GetBuildTargetExtension(bundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
			}

			Debug.Log("<b>WeaverCore Compile Done</b>");
		}
	}
}
