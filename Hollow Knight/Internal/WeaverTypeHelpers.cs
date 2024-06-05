
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class WeaverTypeHelpers
{
	static Assembly _weaverAssembly;

	public static Assembly WeaverAssembly
	{
		get
		{
			if (_weaverAssembly == null)
			{
				_weaverAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "WeaverCore");
			}
			return _weaverAssembly;
		}
	}

	public static Type GetWeaverType(string typeName)
	{
		return WeaverAssembly.GetType(typeName);
	}

	public static MethodInfo GetWeaverMethod(string typeName, string methodName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
	{
		return WeaverAssembly.GetType(typeName)?.GetMethod(methodName, flags);
	}

	public static PropertyInfo GetWeaverProperty(string typeName, string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
	{
		return WeaverAssembly.GetType(typeName)?.GetProperty(propertyName, flags);
	}

	public static FieldInfo GetWeaverField(string typeName, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
	{
		return WeaverAssembly.GetType(typeName)?.GetField(fieldName, flags);
	}

	static Type eventManagerType;
	static MethodInfo broadcastEventMethod;
	static MethodInfo sendEventToGameObjectMethod;

	public static void BroadcastEvent(string eventName, GameObject source)
	{
		if (eventManagerType == null)
		{
			eventManagerType = GetWeaverType("WeaverCore.EventManager");
		}

		if (broadcastEventMethod == null)
		{
			broadcastEventMethod = eventManagerType.GetMethod("BroadcastEvent");
		}

		broadcastEventMethod.Invoke(null,new object[]{ eventName, source });
	}

	public static void SendEventToGameObject(string eventName, GameObject destination, GameObject source = null)
	{
		if (sendEventToGameObjectMethod == null)
		{
			sendEventToGameObjectMethod = eventManagerType.GetMethods().FirstOrDefault(m => m.Name == "SendEventToGameObject" && m.GetParameters().Length == 3);
		}

		sendEventToGameObjectMethod.Invoke(null, new object[]{ eventName, destination, source });
	}
}