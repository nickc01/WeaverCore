



using System;
using System.Reflection;
/*using System.Runtime.InteropServices;
using WeaverCore.Utilities;

namespace WeaverCore
{
    public class OptionalAssembly
    {
        public Assembly Assembly { get; private set; }
        public readonly string AssemblyName;
        public bool Loaded => Assembly != null;

        public OptionalAssembly(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        private void load()
        {
            if (Loaded)
            {
                return;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == AssemblyName || assembly.GetName().Name == AssemblyName)
                {
                    Assembly = assembly;
                    return;
                }
            }

            WeaverLog.Log
        }


        public static MethodInfo GetMethod(string method, BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
        {
            load();
        }

        public static object CallMethod(string method, BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
        {

        }
    }
}*/