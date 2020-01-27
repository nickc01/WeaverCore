using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ViridianLink.Editor.Visual
{
	public static class Builder
	{
		[MenuItem("ViridianLink/Compile %F5")]
		public static void Compile()
		{
			try
			{
				var binFolder = new DirectoryInfo("ViridianLink/bin").FullName;
				//string destination = binFolder + "/" + PlayerSettings.productName.Replace(" ", "") + ".dll";
				//Debug.Log("Original Destination = " + destination);
				if (!Directory.Exists(binFolder))
				{
					Directory.CreateDirectory(binFolder);
				}
				//destination = EditorUtility.OpenFolderPanel("Select a folder to compile to", destination, "");
				//destination = EditorUtility.OpenFilePanel("Select where you want to compile the mod", destination, "");
				//EditorUtility.Save
				string destination = EditorUtility.SaveFilePanel("Select where you want to compile the mod", binFolder, PlayerSettings.productName.Replace(" ", ""), "dll");
				if (destination == "")
				{
					return;
				}
				Debug.Log("New Destination = " + destination);
				Debug.Log("Compiling");
				var assetsFolder = new DirectoryInfo("Assets").FullName;

				var scripts = Directory.GetFiles(assetsFolder, "*.cs", SearchOption.AllDirectories);
				var references = Directory.GetFiles(assetsFolder, "*.dll", SearchOption.AllDirectories);

				AssemblyBuilder builder = new AssemblyBuilder(destination, scripts);

				builder.buildTarget = BuildTarget.StandaloneWindows;

				builder.additionalReferences = references;

				builder.buildTargetGroup = BuildTargetGroup.Standalone;

				Action<string, CompilerMessage[]> finish = null;
				finish = (dest, messages) =>
				{
					bool errors = false;
					foreach (var message in messages)
					{
						switch (message.type)
						{
							case CompilerMessageType.Error:
								Debug.LogError(message.message);
								errors = true;
								break;
							case CompilerMessageType.Warning:
								Debug.LogWarning(message.message);
								break;
						}
					}
					builder.buildFinished -= finish;
					if (!errors)
					{
						PostBuild(dest);
					}
				};

				builder.buildFinished += finish;
				builder.Build();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

		}


		static void PostBuild(string file)
		{

		}
	}

}