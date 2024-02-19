using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Used for building DLL assemblies in WeaverCore
	/// </summary>
	public class AssemblyCompiler
	{
		/// <summary>
		/// Contains the results of an assembly build process
		/// </summary>
		public class OutputDetails
		{
			/// <summary>
			/// Was the build a success
			/// </summary>
			public bool Success;
			/// <summary>
			/// The output path of the DLL
			/// </summary>
			public string OutputPath;

			/// <summary>
			/// Any messages that were printed to the logs during the compilation process
			/// </summary>
			public CompilerMessage[] CompilerMessages;
		}

		/// <summary>
		/// The directory the DLL will be built to
		/// </summary>
		public DirectoryInfo BuildDirectory { get; set; }

		/// <summary>
		/// The file name of the built DLL
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Is the DLL currently being built?
		/// </summary>
		public bool Building { get; private set; }

		/// <summary>
		/// A list of script paths to include in the DLL build
		/// </summary>
		public List<string> Scripts { get; set; }

		/// <summary>
		/// A list of #define's to be included in the DLL build
		/// </summary>
		public List<string> Defines { get; set; }

		/// <summary>
		/// A list of assembly reference paths to include in the build process
		/// </summary>
		public List<string> References { get; set; }

		/// <summary>
		/// The target build platform of the build
		/// </summary>
		public BuildTarget Target { get; set; }

		/// <summary>
		/// The target build group of the build
		/// </summary>
		public BuildTargetGroup TargetGroup { get; set; }

		/// <summary>
		/// A list of excluded assembly reference paths
		/// </summary>
		public List<string> ExcludedReferences { get; set; }

		/// <summary>
		/// Some other compiler build flags
		/// </summary>
		public AssemblyBuilderFlags Flags { get; set; }
		/// <summary>
		/// Contains the output information of the compilation. This is null when the <see cref="Build"/> function has not been called yet
		/// </summary>
		public OutputDetails Output { get; private set; }
		public AssemblyCompiler()
		{
			Building = false;
			Scripts = new List<string>();
			Defines = new List<string>();
			References = new List<string>();
			ExcludedReferences = new List<string>();
			Target = BuildTarget.StandaloneWindows;
			TargetGroup = BuildTargetGroup.Standalone;
			Flags = AssemblyBuilderFlags.None;
		}

		public delegate void buildCompleteAction(OutputDetails output);

		void BuildInternal(buildCompleteAction onComplete)
		{
			Output = null;
			if (!BuildDirectory.Exists)
			{
				BuildDirectory.Create();
			}
			var outputPath = PathUtilities.AddSlash(BuildDirectory.FullName) + FileName;
			AssemblyBuilder builder = new AssemblyBuilder(outputPath, Scripts.ToArray());
			builder.additionalDefines = Defines.ToArray();
			builder.additionalReferences = References.ToArray();
			builder.buildTarget = Target;
			builder.buildTargetGroup = TargetGroup;
			builder.excludeReferences = VerifyPaths(ExcludedReferences);
			builder.flags = Flags;
			builder.compilerOptions = new ScriptCompilerOptions
			{
				AllowUnsafeCode = true,
				CodeOptimization = CodeOptimization.Release
			};
			Action<string, CompilerMessage[]> buildCompleteAction = null;
			var outputInfo = new OutputDetails();
			outputInfo.OutputPath = outputPath;
			buildCompleteAction = (dest, messages) =>
			{
				outputInfo.CompilerMessages = messages;
				Building = false;
				if (messages.Any(cm => cm.type == CompilerMessageType.Error))
				{
					Debug.LogError("Error building assembly = " + FileName);
					string assemblyReferences = "References: " + Environment.NewLine;
					foreach (var reference in References.Concat(GetDefaultReferences()))
					{
						assemblyReferences += reference + Environment.NewLine;
					}
					Debug.LogError(assemblyReferences);

					string assemblyExclusions = "Exclusions: " + Environment.NewLine;
					foreach (var exclusion in ExcludedReferences)
					{
						assemblyExclusions += exclusion + Environment.NewLine;
					}
					Debug.LogError(assemblyExclusions);
					outputInfo.Success = false;
					foreach (var message in messages)
					{
						if (message.type == CompilerMessageType.Error)
						{
							Debug.LogError(message.message);
						}
					}
				}
				else
				{
					outputInfo.Success = true;
				}
				builder.buildFinished -= buildCompleteAction;
				Output = outputInfo;
				if (onComplete != null)
				{
					onComplete(Output);
				}
			};
			Building = true;
			try
			{
				builder.buildFinished += buildCompleteAction;
				builder.Build();
			}
			catch (Exception)
			{
				builder.buildFinished -= buildCompleteAction;
				Building = false;
				throw;
			}
		}

		public IEnumerator Build(OutputDetails details)
		{
			if (details == null)
			{
				throw new Exception("The details parameter cannot be null");
			}
			BuildInternal(null);
			yield return new WaitUntil(() => !Building);
			yield return null;
			details.Success = Output.Success;
			details.OutputPath = Output.OutputPath;
			details.CompilerMessages = Output.CompilerMessages;
		}

		/// <summary>
		/// Verifies all the input paths to make sure they work with the assembly compiler
		/// </summary>
		static string[] VerifyPaths(List<string> paths)
		{
			string[] output = new string[paths.Count  * 2];
			int i = 0;
			foreach (var path in paths)
			{
				if (path.Contains("\\"))
				{
					output[i] = path.Replace("\\", "/");
				}
				else if (path.Contains("//"))
				{
					output[i] = path.Replace("/", "\\");
				}
				else
				{
					output[i] = path;
				}
				output[i + 1] = path;
				i += 2;
			}
			return output;
		}

		/// <summary>
		/// Gets a list of references that unity includes by default when building an assembly
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetDefaultReferences()
		{
			var outputPath = BuildDirectory.FullName + FileName;
			AssemblyBuilder builder = new AssemblyBuilder(outputPath, Scripts.ToArray());
			builder.additionalDefines = Defines.ToArray();
			builder.additionalReferences = References.ToArray();
			builder.buildTarget = Target;
			builder.buildTargetGroup = TargetGroup;
			builder.excludeReferences = ExcludedReferences.ToArray();
			builder.flags = Flags;

			var defaultRefMethod = typeof(UnityEditor.AI.NavMeshBuilder).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilation").GetMethod("GetAssemblyBuilderDefaultReferences", new Type[] { typeof(UnityEditor.Compilation.AssemblyBuilder) });

			var instance = typeof(UnityEditor.AI.NavMeshBuilder).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface").GetProperty("Instance").GetValue(null);

			var defaultReferences = (string[])defaultRefMethod.Invoke(instance, new object[] { builder });

			for (int i = 0; i < defaultReferences.GetLength(0); i++)
			{
				yield return defaultReferences[i];
			}
		}

		/// <summary>
		/// Removes any references to editor assemblies
		/// </summary>
		public void RemoveEditorReferences()
		{
			var defaultReferences = GetDefaultReferences();

			foreach (var dRef in defaultReferences)
			{
				var file = new FileInfo(dRef);
				if (file.Name.Contains("Editor"))
				{
					ExcludedReferences.Add(dRef);
				}
			}
		}

		/// <summary>
		/// Adds in references to Unity assemblies
		/// </summary>
		public void AddUnityReferences()
		{
			var coreModuleLocation = new FileInfo(typeof(MonoBehaviour).Assembly.Location);
			var unityAssemblyFolder = coreModuleLocation.Directory;
			var managedFolder = unityAssemblyFolder.Parent;


			ExcludedReferences.Add(managedFolder.FullName + @"\UnityEngine.dll");
			ExcludedReferences.Add(PathUtilities.ReplaceSlashes(managedFolder.FullName + @"\UnityEngine.dll"));

			foreach (var hkFile in unityAssemblyFolder.EnumerateFiles("*.dll", SearchOption.TopDirectoryOnly))
			{
				if (hkFile.Name.Contains("UnityEngine"))
				{
					References.Add(PathUtilities.ReplaceSlashes(hkFile.FullName));
				}
			}
		}
	}
}
