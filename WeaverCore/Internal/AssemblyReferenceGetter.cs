using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using WeaverCore.Interfaces;
using System.Reflection;
using WeaverCore.Attributes;

namespace WeaverCore.Internal
{
    public static class AssemblyReferenceGetter
    {
        public abstract class Impl : IImplementation
        {
            public abstract string[] GetAssemblyReferencesQuick(Assembly assembly);
        }

        static Impl impl;

        public static string[] GetAssemblyReferencesQuick(Assembly assembly)
        {
            if (impl == null)
            {
                impl = ImplFinder.GetImplementation<Impl>();
            }

            return impl.GetAssemblyReferencesQuick(assembly);
        }
    }
}