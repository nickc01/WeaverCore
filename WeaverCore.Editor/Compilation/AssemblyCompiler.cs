using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	public class AssemblyCompiler
	{
		public class OutputDetails
		{
			public bool Success;
			public string OutputPath;
			public CompilerMessage[] CompilerMessages;
		}

		public DirectoryInfo BuildDirectory { get; set; }
		public string FileName { get; set; }
		//public string BuildPath { get; set; }
		//public bool BuildSuccessful { get; private set; }
		public bool Building { get; private set; }
		public List<string> Scripts { get; set; }
		/*public IEnumerable<FileInfo> Scripts
		{
			get
			{
				foreach (var script in ScriptPaths)
				{
					yield return new FileInfo(script);
				}
			}
			set
			{
				ScriptPaths.Clear();
				foreach (var script in value)
				{
					ScriptPaths.Add(script.FullName);
				}
			}
		}*/
		public List<string> Defines { get; set; }
		public List<string> References { get; set; }
		/*public IEnumerable<FileInfo> References
		{
			get
			{
				foreach (var reference in ReferencePaths)
				{
					yield return new FileInfo(reference);
				}
			}
			set
			{
				ReferencePaths.Clear();
				foreach (var reference in value)
				{
					ReferencePaths.Add(reference.FullName);
				}
			}
		}*/
		public BuildTarget Target { get; set; }
		public BuildTargetGroup TargetGroup { get; set; }
		public List<string> ExcludedReferences { get; set; }
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

		/*public static List<FileInfo> GetAllInDirectory(string extension, string dir = null)
		{
			if (dir == null)
			{
				dir = new DirectoryInfo("Assets").FullName;
			}
			var directory = new DirectoryInfo(dir);
			return directory.GetFiles(extension, SearchOption.AllDirectories).ToList();
		}

		public static List<FileInfo> GetAllRuntimeInDirectory(string extension, string dir = null)
		{
			return GetAllInDirectory(extension, dir).Where(f => !f.Directory.FullName.Contains("Editor\\")).ToList();
		}*/

		public delegate void buildCompleteAction(OutputDetails output);

		void BuildInternal(buildCompleteAction onComplete)
		{
			Output = null;
			var outputPath = BuildDirectory.FullName + FileName;
			Debug.Log("Build Output Path = " + outputPath);
			AssemblyBuilder builder = new AssemblyBuilder(outputPath, Scripts.ToArray());
			builder.additionalDefines = Defines.ToArray();
			builder.additionalReferences = References.ToArray();
			builder.buildTarget = Target;
			builder.buildTargetGroup = TargetGroup;
			builder.excludeReferences = VerifyPaths(ExcludedReferences);
			builder.flags = Flags;
			Action<string, CompilerMessage[]> buildCompleteAction = null;
			var outputInfo = new OutputDetails();
			outputInfo.OutputPath = outputPath;
			buildCompleteAction = (dest, messages) =>
			{
				//Debug.Log("---------Dest = " + dest);
				outputInfo.CompilerMessages = messages;
				Building = false;
				if (messages.Any(cm => cm.type == CompilerMessageType.Error))
				{
					outputInfo.Success = false;
					foreach (var message in messages)
					{
						if (message.type == CompilerMessageType.Error)
						{
							Debug.LogError(message.message);
						}
						else
						{
							//Debug.LogWarning(message.message);
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
		}

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
	}
}
