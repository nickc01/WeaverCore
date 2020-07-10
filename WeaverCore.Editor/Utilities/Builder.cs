using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System;
using WeaverCore.Interfaces;
using System.IO;
using System.Linq;
using WeaverCore.Utilities;
using System.Reflection;

namespace WeaverCore.Editor
{
	public class Builder
	{
		class WaitForBuildToFinish : IWeaverAwaiter
		{
			bool waitOneFrame = true;

			Builder Builder;
			public WaitForBuildToFinish(Builder builder)
			{
				Builder = builder;
			}

			bool IWeaverAwaiter.KeepWaiting()
			{
				if (!Builder.Building)
				{
					if (waitOneFrame)
					{
						waitOneFrame = false;
						return true;
					}
					return false;
				}
				return true;
			}
		}


		public string BuildPath { get; set; }
		//public string AssemblyName { get; set; }
		
		public bool BuildSuccessful { get; private set; }
		public bool Building { get; private set; }
		public List<string> ScriptPaths { get; set; }
		public IEnumerable<FileInfo> Scripts
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
		}
		public List<string> Defines { get; set; }
		public List<string> ReferencePaths { get; set; }
		public IEnumerable<FileInfo> References
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
		}
		public BuildTarget Target { get; set; }
		public BuildTargetGroup TargetGroup { get; set; }

		/// <summary>
		/// Excludes dll references from the build. NOTE: You need to use forward slashes in order for the dll to get properly excluded. DO NOT USE BACKSLASHES
		/// </summary>
		public List<string> ExcludedReferences { get; set; }
		public AssemblyBuilderFlags Flags { get; set; }
		public CompilerMessage[] CompilerMessages { get; private set; }

		public Builder()
		{
			BuildSuccessful = false;
			Building = false;
			ScriptPaths = new List<string>();
			Defines = new List<string>();
			ReferencePaths = new List<string>();
			ExcludedReferences = new List<string>();
			Target = BuildTarget.StandaloneWindows;
			TargetGroup = BuildTargetGroup.Standalone;
			Flags = AssemblyBuilderFlags.None;
		}

		public static List<FileInfo> GetAllInDirectory(string extension, string dir = null)
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
			return GetAllInDirectory(extension,dir).Where(f => !f.Directory.FullName.Contains("Editor\\")).ToList();
		}

		public delegate void buildCompleteAction(string buildDestination, CompilerMessage[] compilerMessages);

		public void Build(buildCompleteAction onComplete)
		{
			BuildSuccessful = false;
			AssemblyBuilder builder = new AssemblyBuilder(BuildPath, ScriptPaths.ToArray());
			builder.additionalDefines = Defines.ToArray();
			builder.additionalReferences = ReferencePaths.ToArray();
			builder.buildTarget = Target;
			builder.buildTargetGroup = TargetGroup;
			builder.excludeReferences = ExcludedReferences.ToArray();
			builder.flags = Flags;
			Action<string, CompilerMessage[]> buildCompleteAction = null;
			buildCompleteAction = (dest, messages) =>
			{
				//Debug.Log("---------Dest = " + dest);
				CompilerMessages = messages;
				Building = false;
				if (messages.Any(cm => cm.type == CompilerMessageType.Error))
				{
					BuildSuccessful = false;
					foreach (var message in messages)
					{
						if (message.type == CompilerMessageType.Error)
						{
							Debug.LogError(message.message);
						}
						else
						{
							Debug.LogWarning(message.message);
						}
					}
				}
				else
				{
					BuildSuccessful = true;
				}
				builder.buildFinished -= buildCompleteAction;
				if (onComplete != null)
				{
					onComplete(dest, messages);
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

		public IWeaverAwaiter Build()
		{
			Build(null);
			return new WaitForBuildToFinish(this);
		}
	}
}
