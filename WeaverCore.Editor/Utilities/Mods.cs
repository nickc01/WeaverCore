/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using WeaverCore.Internal;

namespace WeaverCore.Editor.Utilities
{
    public static class Mods
	{
        public static List<Type> GetMods()
		{
            var mods = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(WeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface)
                    {
                        //if (WeaverAssetsInfo.InWeaverAssetsProject || !typeof(WeaverCore.Internal.WeaverCore).IsAssignableFrom(type))
                        //{
                            mods.Add(type);
                        //}
                    }
                }
            }
            mods.Sort(new TypeComparer());
            return mods;
        }

        public static string[] GetModNames(List<Type> mods)
        {
            var modNames = new string[mods.Count];
            for (int i = 0; i < mods.Count; i++)
            {
                modNames[i] = ObjectNames.NicifyVariableName(mods[i].Name);
            }
            return modNames;
        }

        public static WeaverMod CreateMod(Type type)
        {
            return (WeaverMod)Activator.CreateInstance(type);
        }

        public static bool FindMod(string typeName, string assemblyName,List<Type> modList, out Type result)
        {
            result = FindMod(typeName, assemblyName, modList);
            return result != null;
        }

        public static bool FindMod(string typeName, string assemblyName,out Type result)
        {
            return FindMod(typeName, assemblyName, GetMods(), out result);
        }

        public static Type FindMod(string typeName,string assemblyName,List<Type> modList)
        {
            foreach (var mod in modList)
            {
                if (mod.FullName == typeName && (assemblyName == "" || mod.Assembly.GetName().Name == assemblyName))
                {
                    return mod;
                }
            }
            return null;
        }

        public static Type FindMod(string typeName, string assemblyName)
        {
            return FindMod(typeName, assemblyName, GetMods());
        }
	}
}
*/