using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Helpers;
using WeaverCore.Editor.Routines;
using WeaverCore.Helpers;
using WeaverCore.Internal;

namespace WeaverCore.Editor.Visual
{
	public static class Builder
	{
		static Assembly AssemblyManipulator;

		delegate void embedMethod(string sourceAssembly, string additionFile, string resourcePath, bool deleteSource);
		delegate void addModMethod(string assembly, string @namespace, string typeName,string modName, bool unloadable);

		static embedMethod Embed;

		static addModMethod AddMod;

		const string asmDefFile = "{\n\"name\": \"def\"\n}\n";

		static float progress = 0.0f;

		static string ModName = "";

		static float Progress
		{
			get => progress;
			set
			{
				progress = value;
				EditorUtility.DisplayProgressBar("Compiling", "Compiling Mod : " + ModName, progress);
			}
		}

		static readonly ReadOnlyCollection<BuildMode> buildModes = new List<BuildMode>()
		{
			new BuildMode()
			{
				Extension = ".bundle.win",
				Target = BuildTarget.StandaloneWindows
			},
			new BuildMode()
			{
				Extension = ".bundle.mac",
				Target = BuildTarget.StandaloneOSX
			},
			new BuildMode()
			{
				Extension = ".bundle.unix",
				Target = BuildTarget.StandaloneLinuxUniversal
			}
		}.AsReadOnly();

		static void ShowProgress()
		{
			Progress = progress;
		}

		static void ClearProgress()
		{
			EditorUtility.ClearProgressBar();
		}

		[MenuItem("WeaverCore/Compile %F5")]
		public static void Compile()
		{
			ModNameSelector.ChooseString((modName) =>
			{
				EditorRoutine.Start(BeginCompile(modName));
			});
		}

		public static void ClearLogConsole()
		{
			var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
			Type logEntries = assembly.GetType("UnityEditor.LogEntries");
			MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
			clearConsoleMethod.Invoke(new object(), null);
		}

		static string SelectSaveLocation(string filename)
		{
			string buildFolder;
			if (File.Exists("LastUsedDirectory.dat"))
			{
				buildFolder = File.ReadAllText("LastUsedDirectory.dat");
			}
			else
			{
				buildFolder = new DirectoryInfo($"{nameof(WeaverCore)}/bin").FullName;
			}
			if (!Directory.Exists(buildFolder))
			{
				Directory.CreateDirectory(buildFolder);
			}
			var fileLocation = EditorUtility.SaveFilePanel("Select where you want to compile the mod", buildFolder, filename, "dll");
			if (fileLocation == "")
			{
				return "";
			}
			var selectedFolder = new FileInfo(fileLocation).Directory.FullName;

			using (var file = File.Open("LastUsedDirectory.dat", FileMode.Create))
			{
				using (var writer = new StreamWriter(file))
				{
					writer.Write(selectedFolder);
				}
			}
			return fileLocation;
		}

		static string[] GetFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo($"Assets\\{nameof(WeaverCore)}").FullName;
			var AssetsFolder = new DirectoryInfo("Assets").FullName;
			var files = Directory.GetFiles(AssetsFolder, filter, SearchOption.AllDirectories).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				if (files[i].Contains(EditorDirectory))
				{
					files.RemoveAt(i);
				}
			}
			return files.ToArray();
		}

		static string[] GetScripts()
		{
			return GetFiles("*.cs");
		}

		static string[] GetReferences()
		{
			return GetFiles("*.dll");
		}

		public static IEnumerator<IEditorWaiter> BeginCompile(string modName)
		{
			ModName = modName;
			bool doneCompiling = false;
			string buildDestination = "";
			UnityEditor.Compilation.CompilerMessage[] messages = null;

			string destination = SelectSaveLocation(modName);

			string modFileName = new FileInfo(destination).Name.Replace(".dll", "");
			Debugger.Log("Mod FIle Name = " + modFileName);

			/*using (var file = File.Create("Assets\\ModName.asmdef"))
			{
				using (var writer = new StreamWriter(file))
				{
					writer.Write(asmDefFile.Replace("def", modFileName));
				}
			}
			AssetDatabase.ImportAsset("Assets\\ModName.asmdef",ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);*/

			if (destination == "")
			{
				yield break;
			}

			Progress = 0.0f;

			var builder = new UnityEditor.Compilation.AssemblyBuilder(destination, GetScripts());

			builder.buildTarget = BuildTarget.StandaloneWindows;

			builder.additionalReferences = GetReferences();

			builder.buildTargetGroup = BuildTargetGroup.Standalone;

			Action<string, UnityEditor.Compilation.CompilerMessage[]> finish = null;
			finish = (dest, m) =>
			{
				buildDestination = dest;
				messages = m;
				doneCompiling = true;
			};

			builder.buildFinished += finish;
			builder.Build();

			yield return new WaitTillTrue(() => doneCompiling);
			yield return null;

			bool errors = false;
			foreach (var message in messages)
			{
				switch (message.type)
				{
					case UnityEditor.Compilation.CompilerMessageType.Error:
						Debug.LogError(message.message);
						errors = true;
						break;
					case UnityEditor.Compilation.CompilerMessageType.Warning:
						Debug.LogWarning(message.message);
						break;
				}
			}
			builder.buildFinished -= finish;
			ClearProgress();
			if (errors)
			{
				yield break;
			}
			Progress = 0.1f;
			PostBuild(buildDestination);
		}
		
		//Loads the resource embedder and returns a method to execute it
		static void LoadAssemblyManipulator()
		{
			if (AssemblyManipulator == null)
			{
				//Find the Assembly Loader Type
				var assemblyLoader = typeof(Builder).Assembly.GetType("Costura.AssemblyLoader");

				//Find the method that resolves the assembly
				var resolver = assemblyLoader.GetMethod("ResolveAssembly", BindingFlags.Public | BindingFlags.Static);

				//Resolve the assembly. Mono.Cecil should be loaded beyond this point
				resolver.Invoke(null, new object[] { null, new ResolveEventArgs("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e") });

				//Load the resource embedder now that mono.cecil is loaded
				AssemblyManipulator = AppDomain.CurrentDomain.Load("AssemblyManipulator");
				//Find the main program type
				var program = AssemblyManipulator.GetType("AssemblyManipulator.Program");

				//Find the main method
				var mainMethod = program.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

				//Create a delegate to the main method
				var mainMethodRaw = (Func<string[], int>)Delegate.CreateDelegate(typeof(Func<string[], int>), null, mainMethod);

				Embed = (source, addition, resourcePath, deleteSource) =>
				{
					mainMethodRaw(new string[] { source, addition, resourcePath, deleteSource.ToString() });
				};

				var addModM = program.GetMethod("AddMod",BindingFlags.NonPublic | BindingFlags.Static);

				AddMod = (addModMethod)Delegate.CreateDelegate(typeof(addModMethod), null, addModM);
			}
		}

		//static void ReplaceStringInFile(string source, string replacement,string path)
		//{
			//var data = File.ReadAllText(path);
			//data = data.Replace(source, replacement);
			//File.WriteAllText(path, data);
			/*long originalPosition = stream.Position;
			stream.Position = 0;
			using (var reader = new StreamReader(stream))
			{
				string data = reader.ReadToEnd();
				data.Replace(source, replacement);
			}*/
			/*using (FileStream ouputStream = File.Open("output.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.ReadWrite))
			{
				using (var output = new StreamWriter(ouputStream))
				{
					output.WriteLine($"Replacing {source} with {replacement}");
					char[] sourceChars = new char[] { 'A' };


					long originalPosition = stream.Position;
					stream.Position = 0;
					while (stream.Position < stream.Length)
					{
						char input = (char)stream.ReadByte();
						output.WriteLine("Input = " + input);
						if (input == sourceChars[0])
						{
							if (sourceChars.GetLength(0) > 1)
							{
								bool found = true;
								for (int c = 1; c < sourceChars.GetLength(0); c++)
								{
									if (stream.Position >= stream.Length)
									{
										found = false;
										break;
									}
									var data = (char)stream.ReadByte();
									output.WriteLine("Data = " + data);
									if (sourceChars[c] != data)
									{
										output.WriteLine("False");
										found = false;
										stream.Position -= 1 + c;
										break;
									}
								}
								if (found)
								{
									output.WriteLine("Found Replacement");
									stream.Position -= sourceChars.GetLength(0);
									foreach (var character in sourceChars)
									{
										stream.WriteByte((byte)character);
									}
								}
							}
							else
							{
								stream.Position--;
								stream.WriteByte((byte)sourceChars[0]);
							}
						}
					}
					stream.Position = originalPosition;
				}
			}*/
		//}

		/*static string ReadStringToEnd(Stream stream,char terminator = (char)0)
		{
			string final = "";
			while (true)
			{
				char character = (char)stream.ReadByte();
				if (character == terminator)
				{
					return final;
				}
				else
				{
					final += character;
				}
			}
		}

		static uint ReadInt32(Stream stream)
		{
			byte[] buffer = new byte[4];
			stream.Read(buffer, 0, 4);
			Array.Reverse(buffer);
			return BitConverter.ToUInt32(buffer, 0);
		}

		static ulong ReadInt64(Stream stream)
		{
			byte[] buffer = new byte[8];
			stream.Read(buffer, 0, 8);
			Array.Reverse(buffer);
			return BitConverter.ToUInt64(buffer, 0);
		}*/

		//static void PostProcessBundle(string builtAssemblyPath, string bundleFilePath, string bundleFileName, string bundleName)
		//{
			//ReplaceStringInFile("Assembly-CSharp", ModName, bundleFilePath);
			/*string data = "";
			Debugger.Log("Built Assembly Path = " + builtAssemblyPath);
			Debugger.Log("Bundle File Path = " + bundleFilePath);
			Debugger.Log("Bundle File Name = " + bundleFileName);
			Debugger.Log("Bundle Name = " + bundleName);
			//ReplaceStringInFile("Assembly-CSharp", ModName, bundleFilePath);
			using (FileStream stream = File.Open(bundleFilePath,FileMode.Open,FileAccess.ReadWrite))
			{
				string assetBundleType = ReadStringToEnd(stream);
				uint bundleVersion = ReadInt32(stream);
				string versionTag = ReadStringToEnd(stream);
				string UnityVersion = ReadStringToEnd(stream);
				ulong fileSize = ReadInt64(stream);

				Debugger.Log("Asset Bundle Type = " + assetBundleType);
				Debugger.Log("Bundle Version = " + bundleVersion);
				Debugger.Log("Version Tag = " + versionTag);
				Debugger.Log("Unity Version = " + UnityVersion);
				Debugger.Log("File Size = " + fileSize);
				//stream.Position += 8;
				//byte[] initialBuffer = new byte[16];
				//stream.Read(initialBuffer, 0, 4);
				//int BundleVersion = BitConverter.ToInt32(initialBuffer, 0);
				//stream.Read(initialBuffer, 0, 6);
				//string VersionTag = BitConverter.ToString(initialBuffer);

				//Debugger.Log("Stream Size = " + stream.Length);
				byte[] buffer = new byte[200];
				int amount = 0;
				do
				{
					amount = stream.Read(buffer, 0, buffer.GetLength(0));
					for (int i = 0; i < amount; i++)
					{
						char character = (char)buffer[i];
						if (character >= 32 && character <= 126)
						{
							data += character;
						}
					}
				} while (amount > 0);
			}
			Debugger.Log($"Bundle Data for {bundleName} = {data}");*/
			//File.WriteAllText(data, bundleName + "_dump.dat");
		//}

		static void AdjustMonoScripts()
		{
			//Debug.Log("Adjusting scripts...");

			IEnumerable<string> assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".cs") && !path.Contains("Assets/Editor"));

			foreach (var path in assetFolderPaths)
			{
				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

				if (script != null)
				{
					ChangeMonoScriptAssembly(script, ModName);
				}
			}
		}

		static void ChangeMonoScriptAssembly(MonoScript script, string assemblyName)
		{
			MethodInfo getNamespace = script.GetType().GetMethod("GetNamespace", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo dynMethod = script.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);
			dynMethod.Invoke(script, new object[] { script.text, script.name, (string)getNamespace.Invoke(script,null), assemblyName, false });
		}

		static void PostBuild(string builtAssemblyPath)
		{
			List<Type> ValidMods = Assembly.Load("Assembly-CSharp").GetTypes().Where(type => typeof(IWeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface && !typeof(WeaverCoreMod).IsAssignableFrom(type)).ToList();
			AdjustMonoScripts();
			LoadAssemblyManipulator();
			var temp = Path.GetTempPath();
			for (int modeIndex = 0; modeIndex < buildModes.Count; modeIndex++)
			{
				Progress = Mathf.Lerp(0.1f, 1.0f, modeIndex / (float)buildModes.Count);
				var mode = buildModes[modeIndex];
				if (IsPlatformSupportLoaded(mode.Target))
				{
					var bundleBuilds = new DirectoryInfo(temp + @"BundleBuilds\");
					if (bundleBuilds.Exists)
					{
						foreach (var existingFile in bundleBuilds.GetFiles())
						{
							existingFile.Delete();
						}
					}
					else
					{
						bundleBuilds.Create();
					}
					BuildPipeline.BuildAssetBundles(bundleBuilds.FullName, BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle, mode.Target);
					ShowProgress();
					foreach (var bundleFile in bundleBuilds.GetFiles())
					{
						if (bundleFile.Extension == "")
						{
							//PostProcessBundle(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension,bundleFile.Name);
							Embed(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension, false);
						}
					}
				}
				else
				{
					Debug.LogWarning($"{mode.Target} module is not loaded, so building for the target is not available");
				}
			}
			ClearProgress();
			foreach (var modType in ValidMods)
			{
				var instance = Activator.CreateInstance(modType) as IWeaverMod;
				AddMod(builtAssemblyPath, modType.Namespace, modType.Name,instance.Name,instance.Unloadable);
			}
			Debug.Log("Build Complete");
		}

		struct BuildMode
		{
			public string Extension;
			public BuildTarget Target;
		}

		//Tests if a build target is available
		public static bool IsPlatformSupportLoaded(BuildTarget buildTarget)
		{
			var UnityEditor = System.Reflection.Assembly.Load("UnityEditor");	
			var ModuleManagerT = UnityEditor.GetType("UnityEditor.Modules.ModuleManager");

			var buildString = (string)ModuleManagerT.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildTarget });
			return (bool)ModuleManagerT.GetMethod("IsPlatformSupportLoaded", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildString });

		}
	}

}