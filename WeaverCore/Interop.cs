using Modding;
using MonoMod.ModInterop;
using System.Linq;
using System.Reflection;

namespace WeaverCore
{
    public static class Interop
    {
        public static IMod GetModByName(string modName)
        {
            var mod = WeaverMod.LoadedMods.FirstOrDefault(m => m.GetName() == modName);
            if (mod == default)
            {
                mod = WeaverMod.LoadedMods.FirstOrDefault(m => m.GetType().Name == modName);
            }

            if (mod == default)
            {
                mod = WeaverMod.LoadedMods.FirstOrDefault(m => m.GetName().Contains(modName));
            }

            if (mod == default)
            {
                mod = WeaverMod.LoadedMods.FirstOrDefault(m => m.GetType().Name.Contains(modName));
            }
            return mod;
        }

        public static Assembly GetModAssembly(string modName)
        {
            return GetModByName(modName).GetType().Assembly;
        }
	}
}
