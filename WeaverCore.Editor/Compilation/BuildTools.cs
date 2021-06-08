using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Compilation
{
	public static class BuildTools
	{
		class BuildTask<T> : IAsyncBuildTask<T>
		{
			public T Result { get; set; }

			public bool Completed { get; set; }

			object IAsyncBuildTask.Result { get { return Result; } set { Result = (T)value; } }
		}

		class BuildParameters
		{
			public DirectoryInfo BuildDirectory;
			public string FileName;

			public FileInfo BuildPath
			{
				get
				{
					return new FileInfo(BuildDirectory.FullName + FileName);
				}
				set
				{
					BuildDirectory = value.Directory;
					FileName = value.Name;
				}
			}

			public List<string> Scripts = new List<string>();
			public List<string> AssemblyReferences = new List<string>();
			public List<string> ExcludedReferences = new List<string>();
			public List<string> Defines = new List<string>();

			public BuildTarget Target = BuildTarget.StandaloneWindows;
			public BuildTargetGroup Group = BuildTargetGroup.Standalone;
		}

		public class BuildOutput
		{
			public bool Success;
			public List<FileInfo> OutputFiles = new List<FileInfo>();
		}

		/// <summary>
		/// Builds a WeaverCore DLL without any resources embedded into it. This also runs <see cref="BuildHollowKnightAsm(FileInfo)"/> and puts it into the same output directory, since WeaverCore depends on it
		/// </summary>
		/// <returns>The output file</returns>
		public static IAsyncBuildTask<BuildOutput> BuildPartialWeaverCore(FileInfo outputPath)
		{
			Debug.LogError("B");
			var task = new BuildTask<BuildOutput>();
			Debug.LogError("C");
			UnboundCoroutine.Start(BuildPartialWeaverCoreRoutine(outputPath,task));
			Debug.LogError("D");
			return task;
		}

		static IEnumerator BuildPartialWeaverCoreRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			Debug.LogError("E");
			task.Result = new BuildOutput();
			var buildDirectory = outputPath.Directory;
			var hkFile = new FileInfo(buildDirectory.FullName + "Assembly-CSharp.dll");
			Debug.LogError("F");
			var hkBuildTask = BuildHollowKnightAsm(hkFile);
			Debug.LogError("R");
			yield return new WaitUntil(() => hkBuildTask.Completed);
			Debug.LogError("S");
			task.Result.Success = hkBuildTask.Result.Success;
			task.Result.OutputFiles = hkBuildTask.Result.OutputFiles;
			if (!hkBuildTask.Result.Success)
			{
				Debug.LogError("T");
				task.Completed = true;
				yield break;
			}
			/*
			    weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.Editor.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverBuildTools.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/TMPro.Editor.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/0Harmony.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/Mono.Cecil.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/JUNK.dll");
			 */
			Debug.LogError("U");
			yield return BuildAssembly(new BuildParameters
			{
				BuildPath = outputPath,
				AssemblyReferences = new List<string>
				{
					hkFile.FullName
				},
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/WeaverCore.dll",
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/WeaverCore.Editor.dll",
					"Library/ScriptAssemblies/WeaverBuildTools.dll",
					"Library/ScriptAssemblies/TMPro.Editor.dll",
					"Library/ScriptAssemblies/0Harmony.dll",
					"Library/ScriptAssemblies/Mono.Cecil.dll",
					"Library/ScriptAssemblies/JUNK.dll"
				},
				Scripts = ScriptFinder.FindAssemblyScripts("WeaverCore")
			},task);
			Debug.LogError("V");
			Debug.Log("PARTIAL WEAVERCORE COMPLETE");
		}

		/// <summary>
		/// Builds a DLL using the scripts from WeaverCore/Hollow Knight
		/// </summary>
		/// <param name="outputPath">The path the file will be placed</param>
		/// <returns></returns>
		public static IAsyncBuildTask<BuildOutput> BuildHollowKnightAsm(FileInfo outputPath)
		{
			Debug.LogError("G");
			var task = new BuildTask<BuildOutput>();
			Debug.LogError("H");
			UnboundCoroutine.Start(BuildHollowKnightAsmRoutine(outputPath, task));
			Debug.LogError("I");
			return task;
		}

		static IEnumerator BuildHollowKnightAsmRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			/*task.Result = new BuildOutput();
			var builder = new AssemblyCompiler();
			builder.BuildDirectory = outputPath.Directory;
			builder.FileName = outputPath.Name;
			builder.Scripts = ScriptFinder.FindAssemblyScripts("HollowKnight");
			builder.Defines.Add("GAME_BUILD");
			builder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");
			builder.ExcludedReferences.Add("Library/ScriptAssemblies/JUNK.dll");*/


			//yield return builder.Build(output);
			Debug.LogError("J");
			yield return BuildAssembly(new BuildParameters
			{
				BuildPath = outputPath,
				Scripts = ScriptFinder.FindAssemblyScripts("HollowKnight"),
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/JUNK.dll"
				}
			}, task);
			Debug.LogError("K");
			Debug.Log("HK BUILD COMPLETE");
		}

		public static IAsyncBuildTask<BuildOutput> BuildPartialMod(FileInfo outputPath)
		{
			return null;
		}

		static IEnumerator BuildAssembly(BuildParameters parameters, BuildTask<BuildOutput> task)
		{
			Debug.LogError("L");
			task.Result = new BuildOutput();
			var builder = new AssemblyCompiler();
			builder.BuildDirectory = parameters.BuildDirectory;
			builder.FileName = parameters.FileName;
			builder.Scripts = parameters.Scripts;
			builder.Target = parameters.Target;
			builder.TargetGroup = parameters.Group;
			builder.References = parameters.AssemblyReferences;
			builder.ExcludedReferences = parameters.ExcludedReferences;
			builder.Defines = parameters.Defines;

			if (!parameters.BuildPath.Directory.Exists)
			{
				parameters.BuildPath.Directory.Create();
			}
			if (parameters.BuildPath.Exists)
			{
				parameters.BuildPath.Delete();
			}

			Debug.LogError("M");
			AssemblyCompiler.OutputDetails output = new AssemblyCompiler.OutputDetails();
			Debug.LogError("N");
			yield return builder.Build(output);
			Debug.LogError("O");
			if (output.Success)
			{
				Debug.LogError("P");
				task.Result.OutputFiles.Add(parameters.BuildPath);
			}
			Debug.LogError("Q");
			task.Result.Success = output.Success;
			task.Completed = true;
		}


		[OnInit]
		static void Init()
		{

			Debug.LogError("A");

			Debug.LogError("B");
			//BuildPartialWeaverCore(new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll"));
			//BuildTask<BuildOutput> test = new BuildTask<BuildOutput>();
			//Debug.Log("Starting WeaverCore Build");
			//Debug.Log("Unity Version = " + Application.unityVersion);
			//UnboundCoroutine.Start(BuildPartialWeaverCoreRoutine(new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll"), test));
		}
	}
}
