using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VoidCore
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetTypesSafe(this Assembly assembly)
        {
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (var type in types)
            {
                if (type != null)
                {
                    yield return type;
                }
            }
        }
    }
}
