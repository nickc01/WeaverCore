using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Utilities
{
	public abstract class SimplePatcher<ClassToPatch> : SimplePatcher
	{
		protected sealed override Type GetClassToPatch()
		{
			return typeof(ClassToPatch);
		}
	}

	public abstract class SimplePatcher : IPatch
	{
		protected abstract Type GetClassToPatch();

		void IPatch.Patch(HarmonyPatcher patcher)
		{
			var selfType = GetType();
			var patchType = GetClassToPatch();

			if (patchType == null)
			{
				Debug.LogError("SimplePatcher Error: Patch Class not found");
				return;
			}

			var basePrefix = GetStaticMethod(selfType, "Prefix");
			var basePostfix = GetStaticMethod(selfType, "Postfix");


			foreach (var method in patchType.GetMethods())
			{
				if (!method.IsAbstract && !method.IsGenericMethodDefinition)
				{
					var prefix = GetStaticMethod(selfType, "Prefix" + method.Name);
					var postfix = GetStaticMethod(selfType, "Postfix" + method.Name);

					patcher.Patch(method, prefix ?? basePrefix, postfix ?? basePostfix);


					//Debug.Log("-----Patching Method: " + method.Name + " Prefix: " + GetMethodName(prefix ?? basePrefix) + " Postfix: " + GetMethodName(postfix ?? basePostfix));
				}
			}
		}

		string GetMethodName(MethodInfo method)
		{
			if (method == null)
			{
				return "null";
			}
			else
			{
				return method.Name;
			}
		}

		MethodInfo GetStaticMethod(Type type, string name, bool caseSensitive = true)
		{
			var result = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (result == null && !caseSensitive)
			{
				name = name.ToLower();
				return type.GetMethods().Where(m => m.IsStatic && !m.IsAbstract && !m.IsGenericMethodDefinition && m.Name.ToLower() == name).FirstOrDefault();
			}
			return result;
		}
	}
}
