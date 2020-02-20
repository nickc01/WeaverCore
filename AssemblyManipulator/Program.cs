using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace AssemblyManipulator
{
	class Program
	{
		/*public static void ReplaceResource(string assembly, string resourceName, byte[] resource)
		{
			using (var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
			{
				for (var i = 0; i < definition.MainModule.Resources.Count; i++)
					if (definition.MainModule.Resources[i].Name == resourceName)
					{
						definition.MainModule.Resources.RemoveAt(i);
						break;
					}

				var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
				definition.MainModule.Resources.Add(er);
				definition.MainModule.Write();
			}
		}*/

		public static void AddResource(string assembly, string resourceName, Stream resource)
		{
			double previousTime = GetTime();
			while (GetTime() - previousTime <= 10.0)
			{
				try
				{
					using (var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
					{
						var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
						var finding = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
						if (finding != null)
						{
							definition.MainModule.Resources.Remove(finding);
						}
						//Console.WriteLine("Adding");
						definition.MainModule.Resources.Add(er);
						definition.MainModule.Write();
					}
					break;
				}
				catch (Exception e)
				{
					if (e.Message.Contains("because it is being used by another process"))
					{
						continue;
					}
					else
					{
						throw;
					}
				}
			}
			if (GetTime() - previousTime > 10.0)
			{
				throw new Exception("Embedding Timeout");
			}
		}

		/*public static void AddResource(string assembly, string resourceName, byte[] resource)
		{
			using (var definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
			{
				var er = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, resource);
				var finding = definition.MainModule.Resources.FirstOrDefault(r => r.Name == resourceName);
				if (finding != null)
				{
					definition.MainModule.Resources.Remove(finding);
				}
				//Console.WriteLine("Adding");
				definition.MainModule.Resources.Add(er);
				definition.MainModule.Write();
			}
		}*/

		/*public static MemoryStream GetResource(Stream assembly, string resourceName)
		{
			using (var definition = AssemblyDefinition.ReadAssembly(assembly))
			{
				foreach (var resource in definition.MainModule.Resources)
				{
					if (resource.Name == resourceName)
					{
						var embeddedResource = (EmbeddedResource)resource;
						var memStream = new MemoryStream();
						using (var stream = embeddedResource.GetResourceStream())
						{
							var bytes = new byte[stream.Length];
							stream.Read(bytes, 0, bytes.Length);

							memStream.Write(bytes, 0, bytes.Length);
							memStream.Position = 0;
							stream.Close();
						}
						assembly.Position = 0;
						return memStream;
					}
				}
			}
			assembly.Position = 0;
			return null;
		}*/

		/*public static FileStream OpenFileWhenReady(string filename)
		{
			while (true)
			{
				FileStream stream = null;
				try
				{
					stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
					return stream;
				}
				catch (Exception e)
				{
					if (stream != null)
					{
						stream.Dispose();
					}
					if (e.Message.Contains("because it is being used by another process"))
					{
						continue;
					}
					else
					{
						throw;
					}
				}
			}
		}*/

		static double GetTime()
		{
			return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
		}

		static int Main(string[] args)
		{
			string sourceAssembly = args[0];
			string additionFile = args[1];
			string resourcePath = args[2];

			Console.WriteLine($"Embedding {Path.GetFileName(additionFile)} into {Path.GetFileName(sourceAssembly)} at the resource path: ({resourcePath})...");
			using (var additionStream = File.OpenRead(additionFile))
			{
				AddResource(sourceAssembly, resourcePath, additionStream);
			}
			Console.WriteLine("Embedding Successful!");
			return 0;
		}

		static MethodDefinition FindMethodRecursive(TypeDefinition type,Func<MethodDefinition,bool> predicate,params AssemblyDefinition[] possibleAssemblies)
		{
			var currentType = type;
			while (true)
			{
				var result = currentType.Methods.FirstOrDefault(predicate);
				if (result != null)
				{
					return result;
				}
				else
				{
					var baseType = currentType.BaseType;
					if (baseType.FullName == "System.Object" || baseType.FullName == currentType.FullName)
					{
						return null;
					}
					else
					{
						var reference = currentType.Module.ImportReference(baseType);
						bool found = false;
						foreach (var assembly in possibleAssemblies)
						{
							var definition = assembly.MainModule.GetType(reference.FullName);
							if (definition != null)
							{
								currentType = definition;
								found = true;
								break;
							}
						}
						if (!found)
						{
							currentType = currentType.Module.ImportReference(baseType).Resolve();
						}
					}

				}
			}
		}

		static MethodReference FindMethodRecursiveRef(TypeDefinition type, Func<MethodDefinition, bool> predicate, params AssemblyDefinition[] possibleAssemblies)
		{
			var result = FindMethodRecursive(type, predicate,possibleAssemblies);
			if (result == null)
			{
				throw new Exception($"Could not find method for type {type.FullName}");
			}
			else
			{
				if (result.Module.FileName == type.Module.FileName)
				{
					return result;
				}
				else
				{
					return type.Module.ImportReference(result);
				}
			}
		}

		static PropertyDefinition FindPropertyRecursive(TypeDefinition type, Func<PropertyDefinition, bool> predicate)
		{
			var currentType = type;
			while (true)
			{
				var result = type.Properties.FirstOrDefault(predicate);
				if (result != null)
				{
					return result;
				}
				else
				{
					var baseType = currentType.BaseType;
					if (baseType.FullName == "System.Object" || baseType.FullName == currentType.FullName)
					{
						return null;
					}
					else
					{
						currentType = currentType.Module.ImportReference(baseType).Resolve();
					}

				}
			}
		}

		static void AddModNew(string assembly, string @namespace, string typeName, string modName, bool unloadable)
		{
			string hollowKnightPath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Assembly-CSharp.dll";
			string weaverCorePath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Mods\WeaverCore.dll";
			string coreModulePath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\UnityEngine.CoreModule.dll";
			using (var patcher = new ModPatcher(assembly,hollowKnightPath,weaverCorePath,coreModulePath))
			{
				string newNamespace = @namespace;
				if (newNamespace != null && newNamespace != "")
				{
					newNamespace = ".";
				}
				patcher.Patch(newNamespace + "_WeaverMod_", typeName,newNamespace + typeName,modName);
			}
		}

		static void AddMod(string assembly,string @namespace,string typeName,string modName, bool unloadable)
		{
			string hollowKnightPath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Assembly-CSharp.dll";
			string weaverCorePath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Mods\WeaverCore.dll";
			string coreModulePath = @"C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\UnityEngine.CoreModule.dll";
			using (var unityAssembly = AssemblyDefinition.ReadAssembly(coreModulePath))
			{
				using (var modAssembly = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true }))
				{
					using (var hollowKnightAssembly = AssemblyDefinition.ReadAssembly(hollowKnightPath))
					{
						using (var weaverCoreAssembly = AssemblyDefinition.ReadAssembly(weaverCorePath))
						{
							var modModule = modAssembly.MainModule;
							var hollowModule = hollowKnightAssembly.MainModule;
							var weaverModule = weaverCoreAssembly.MainModule;
							var unityModule = unityAssembly.MainModule;
							var hollowKnightRef = new AssemblyNameReference(hollowKnightAssembly.Name.Name, hollowKnightAssembly.Name.Version);
							//modModule.AssemblyReferences.Add(hollowKnightRef);



							//-----Define Final Mod Type-----
							var iModTDef = hollowModule.GetType("Modding.IMod");
							var modTDef = hollowModule.GetType("Modding.Mod");
							var iLoggerDef = hollowModule.GetType("Modding.ILogger");
							var iModT = modModule.ImportReference(iModTDef);
							var iLoggerT = modModule.ImportReference(iLoggerDef);
							var modT = modModule.ImportReference(modTDef);
							var VModT = modModule.GetType(@namespace, typeName);

							/*string debugInfo = "";
							foreach (var method in VModT.Methods)
							{
								debugInfo += "Method = " + method.Name + "\n";
							}
							foreach (var prop in VModT.Properties)
							{
								debugInfo += "Property = " + prop.Name + "\n";
							}
							throw new Exception(debugInfo);*/
							//string output = "";
							string newNamespace = @namespace;
							if (newNamespace != null && newNamespace != "")
							{
								newNamespace = ".";
							}
							/*if (newNamespace != "" && newNamespace != " ")
							{
								newNamespace += ".";
							}*/
							//output += "Final Namespace = " + newNamespace + "\n";
							//throw new Exception(output);


							var finalModType = new TypeDefinition(newNamespace + "VMod", typeName, TypeAttributes.Public,modT);

							finalModType.Interfaces.Add(new InterfaceImplementation(iModT));
							finalModType.Interfaces.Add(new InterfaceImplementation(iLoggerT));

							finalModType.IsBeforeFieldInit = true;



							//-----Define mod Field-----
							finalModType.Fields.Add(new FieldDefinition("mod", FieldAttributes.Private, VModT));



							//-----Define registry Field-----
							var registryTDef = weaverModule.GetType("WeaverCore.Registry");
							var registryT = modModule.ImportReference(registryTDef);
							finalModType.Fields.Add(new FieldDefinition("registry", FieldAttributes.Private, registryT));


							//-----Define Constructor-----
							var constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
							var constructor = new MethodDefinition(".ctor", constructorAttributes, modModule.ImportReference(typeof(void)));

							var ObjectT = modModule.ImportReference(typeof(object)).Resolve();
							var modTCtor = modTDef.Methods.First(m => m.Name == ".ctor" && m.Parameters.Count == 1);
							//var ObjectConstructorDef = ObjectT.Methods.First(m => m.Name.Contains("ctor") && !m.HasParameters);
							//var ObjectConstructor = modModule.ImportReference(ObjectConstructorDef);
							var VModConstructor = VModT.Methods.First(m => m.Name.Contains("ctor") && !m.HasParameters);
							var GetTypeMDef = ObjectT.Methods.First(m => m.Name == "GetType");
							var GetTypeM = modModule.ImportReference(GetTypeMDef);

							var registryLoaderTDef = weaverModule.GetType("WeaverCore.Helpers.RegistryLoader");
							var registryLoaderT = modModule.ImportReference(registryLoaderTDef);
							var getModRegistryDef = registryLoaderTDef.Methods.First(m => m.Name == "GetModRegistry" && m.Parameters.Count == 1);
							var getModRegistry = modModule.ImportReference(getModRegistryDef);

							var modField = finalModType.Fields.First(f => f.Name == "mod");
							var registryField = finalModType.Fields.First(f => f.Name == "registry");

							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr,modName));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, modModule.ImportReference(modTCtor)));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, VModConstructor));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, modField));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, modField));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, GetTypeM));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getModRegistry));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, registryField));
							constructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

							finalModType.Methods.Add(constructor);

							//-----Define GetVersion() Method-----

							try
							{
								//var vModGetVersion = VModT.Methods.First(m => m.Name.Contains("Version"));
								var vModGetVersion = FindMethodRecursiveRef(VModT, m => m.Name == "get_Version", weaverCoreAssembly, hollowKnightAssembly);

								var GetVersionM = new MethodDefinition("GetVersion", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, modModule.ImportReference(typeof(string)));
								GetVersionM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
								GetVersionM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, modField));
								GetVersionM.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, vModGetVersion));
								GetVersionM.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

								finalModType.Methods.Add(GetVersionM);
							}
							catch (Exception e)
							{
								throw new Exception("B", e);
							}

							var GameObjectRef = modModule.ImportReference(unityModule.GetType("UnityEngine.GameObject"));

							var unityObjectT = unityModule.GetType("UnityEngine.Object");
							var unityObjectRef = modModule.ImportReference(unityObjectT);

							MethodDefinition unityObjectInequality;
							try
							{
								unityObjectInequality = unityObjectT.Methods.First(m => m.Name == "op_Inequality");
							}
							catch (Exception e)
							{
								throw new Exception("AA", e);
							}

							MethodReference setRegistryEnabled;
							try
							{
								setRegistryEnabled = modModule.ImportReference(registryTDef.Methods.First(m => m.Name == "set_RegistryEnabled"));
							}
							catch (Exception e)
							{
								throw new Exception("BB", e);
							}
							MethodReference loadMethod = null;
							try
							{
								//loadMethod = VModT.Methods.First(m => m.Name == "Load");
								loadMethod = FindMethodRecursiveRef(VModT, m => m.Name == "Load", weaverCoreAssembly, hollowKnightAssembly);
							}
							catch (Exception e)
							{
								throw new Exception("CC", e);
							}


							var InitializeM = new MethodDefinition("Initialize", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, modModule.ImportReference(typeof(void)));
							//InitializeM.Parameters.Add(new ParameterDefinition("preloadedObjects", ParameterAttributes.None, finalDictionary1));

							InitializeM.Body.Variables.Add(new VariableDefinition(modModule.ImportReference(typeof(bool))));

							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, registryField));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Call, modModule.ImportReference(unityObjectInequality)));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
							var branchDestination = Instruction.Create(OpCodes.Ldarg_0);
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse_S, branchDestination));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, registryField));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, setRegistryEnabled));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							InitializeM.Body.Instructions.Add(branchDestination);
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, modField));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, loadMethod));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
							InitializeM.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

							finalModType.Methods.Add(InitializeM);



							//-----Define IsCurrent() Method-----

							var isCurrentM = new MethodDefinition("IsCurrent", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, modModule.ImportReference(typeof(bool)));

							isCurrentM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
							isCurrentM.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

							finalModType.Methods.Add(isCurrentM);



							//-----Define LoadPriority() Method-----
							var loadPriorityM = new MethodDefinition("LoadPriority",MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,modModule.ImportReference(typeof(int)));

							var getLoadPriorityRef = FindMethodRecursiveRef(VModT, m => m.Name == "get_LoadPriority", weaverCoreAssembly, hollowKnightAssembly);

							loadPriorityM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
							loadPriorityM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld,modField));
							loadPriorityM.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,getLoadPriorityRef));
							loadPriorityM.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

							finalModType.Methods.Add(loadPriorityM);




							//-----Define Unload() Method-----
							if (unloadable)
							{
								var iTogglableMod = hollowModule.GetType("Modding.ITogglableMod");

								finalModType.Interfaces.Add(new InterfaceImplementation(modModule.ImportReference(iTogglableMod)));


								var UnloadM = new MethodDefinition("Unload", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, modModule.ImportReference(typeof(void)));

								UnloadM.Body.Variables.Add(new VariableDefinition(modModule.ImportReference(typeof(bool))));

								var vModUnload = FindMethodRecursiveRef(VModT, m => m.Name == "Unload", weaverCoreAssembly, hollowKnightAssembly);

								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld,registryField));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Call, modModule.ImportReference(unityObjectInequality)));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
								var destination = Instruction.Create(OpCodes.Ldarg_0);
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse_S,destination));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld,registryField));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,setRegistryEnabled));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
								UnloadM.Body.Instructions.Add(destination);
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld,modField));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,vModUnload));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
								UnloadM.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

								finalModType.Methods.Add(UnloadM);


							}

							modModule.Types.Add(finalModType);
							modModule.Write();
							//modAssembly.Write();
						}
					}
					//module.Types.Add();
				}
			}
		}
	}
}
