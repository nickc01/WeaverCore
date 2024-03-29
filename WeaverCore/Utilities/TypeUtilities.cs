﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains utility functions related to types
	/// </summary>
	public static class TypeUtilities
	{
		static Cache<(string tName, string aName), Type> typeCache = new Cache<(string tName, string aName), Type>();

		/// <summary>
		/// Takes a type name and assembly name and uses it to retrieve a type
		/// </summary>
		/// <param name="fullTypeName">The full name of the type</param>
		/// <param name="assemblyName">The name of the assembly the type is from</param>
		/// <returns>Returns a type with the same name from the assembly</returns>
		public static Type NameToType(string fullTypeName,string assemblyName)
		{
			if (string.IsNullOrEmpty(fullTypeName))
			{
				return null;
			}
			if (typeCache.GetCachedObject((fullTypeName,assemblyName),out var result))
			{
				return result;
			}

			string[] subTypes = fullTypeName.Split('+');

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (string.IsNullOrEmpty(assemblyName) || assembly.FullName == assemblyName || assembly.GetName().Name == assemblyName)
				{
					var type = assembly.GetType(subTypes[0], false);
					if (type != null)
					{
						for (int i = 1; i < subTypes.Length; i++)
						{
							type = type.GetNestedType(subTypes[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
							if (type == null)
							{
								break;
							}
						}

						if (type != null)
						{
                            typeCache.CacheObject((fullTypeName, assemblyName), type);
                            return type;
                        }
					}
				}
			}
			return null;
		}
	}
}
