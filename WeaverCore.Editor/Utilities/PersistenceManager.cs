using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	public static class PersistenceManager
	{
		[InitializeOnLoad]
		class Startup
		{
			static Startup()
			{
				var persistentData = ConfigSettings.Retrieve<PersistenceData>();


				Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					Assemblies.Add(assembly.GetName().Name, assembly);
				}

				for (int i = 0; i < persistentData.functionMethodName.Count; i++)
				{
					try
					{
						var assemblyName = persistentData.functionAssemblyName[i];
						var typeName = persistentData.functionTypeName[i];
						var methodName = persistentData.functionMethodName[i];

						if (!Assemblies.ContainsKey(assemblyName))
						{
							throw new Exception("The assembly " + assemblyName + " does not exist when trying to run method " + methodName + " on type " + typeName);
						}

						var assembly = Assemblies[assemblyName];

						var type = assembly.GetType(typeName);
						if (type == null)
						{
							throw new Exception("The type " + typeName + " could not be found in assembly " + assemblyName);
						}

						var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

						if (method == null)
						{
							throw new Exception("The method " + methodName + " could not be found in type " + typeName + " in assembly " + assemblyName);
						}

						method.Invoke(null, null);
					}
					catch (Exception e)
					{
						Debug.LogError("Error running persistence function: " + e);
					}
				}
				persistentData.functionAssemblyName.Clear();
				persistentData.functionTypeName.Clear();
				persistentData.functionMethodName.Clear();
				persistentData.SetStoredSettings();
			}
		}


		public static void AddFunctionToCall(string assemblyName, string typeName, string methodName)
		{
			var persistentData = ConfigSettings.Retrieve<PersistenceData>();
			persistentData.functionAssemblyName.Add(assemblyName);
			persistentData.functionTypeName.Add(typeName);
			persistentData.functionMethodName.Add(methodName);
			persistentData.SetStoredSettings();
		}

		public static void AddFunctionToCall(MethodInfo function)
		{
			AddFunctionToCall(function.DeclaringType.Assembly.GetName().Name, function.DeclaringType.FullName, function.Name);
		}

		public static void AddData(string key, object data)
		{
			var persistentData = ConfigSettings.Retrieve<PersistenceData>();

			var dataString = JsonUtility.ToJson(data);
			if (persistentData.JsonKeys.Contains(key))
			{
				var index = persistentData.JsonKeys.IndexOf(key);
				persistentData.JsonKeys.RemoveAt(index);
				persistentData.JsonData.RemoveAt(index);
			}
			persistentData.JsonKeys.Add(key);
			persistentData.JsonData.Add(dataString);
			persistentData.SetStoredSettings();
		}

		public static T GetData<T>(string key)
		{
			var persistentData = ConfigSettings.Retrieve<PersistenceData>();
			for (int i = 0; i < persistentData.JsonKeys.Count; i++)
			{
				if (persistentData.JsonKeys[i] == key)
				{
					try
					{
						return JsonUtility.FromJson<T>(persistentData.JsonData[i]);
					}
					catch (Exception)
					{
						return default(T);
					}
				}
			}
			return default(T);
		}

		public static void RemoveData(string key)
		{
			var persistentData = ConfigSettings.Retrieve<PersistenceData>();
			for (int i = 0; i < persistentData.JsonKeys.Count; i++)
			{
				if (persistentData.JsonKeys[i] == key)
				{
					persistentData.JsonKeys.RemoveAt(i);
					persistentData.JsonData.RemoveAt(i);
					persistentData.SetStoredSettings();
					return;
				}
			}
		}





		[Serializable]
		class PersistenceData : ConfigSettings
		{
			public List<string> functionAssemblyName = new List<string>();
			public List<string> functionTypeName = new List<string>();
			public List<string> functionMethodName = new List<string>();

			public List<string> JsonKeys = new List<string>();
			public List<string> JsonData = new List<string>();
		}
	}
}
