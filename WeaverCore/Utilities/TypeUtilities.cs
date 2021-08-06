using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverCore.Utilities
{
	public static class TypeUtilities
	{
		static Cache<(string tName, string aName), Type> typeCache = new Cache<(string tName, string aName), Type>();
		public static Type NameToType(string fullTypeName,string assemblyName)
		{
			if (string.IsNullOrEmpty(fullTypeName) || string.IsNullOrEmpty(assemblyName))
			{
				return null;
			}
			if (typeCache.GetCachedObject((fullTypeName,assemblyName),out var result))
			{
				return result;
			}

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.FullName == assemblyName || assembly.GetName().Name == assemblyName)
				{
					var type = assembly.GetType(fullTypeName);
					if (type != null)
					{
						typeCache.CacheObject((fullTypeName, assemblyName), type);
						return type;
					}
				}
			}
			return null;
		}
	}
}
