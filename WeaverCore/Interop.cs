using Modding;
using MonoMod.ModInterop;
using System.Linq;
using System.Reflection;

namespace WeaverCore
{
    /// <summary>
    /// Used to help with mod interop
    /// </summary>
    public static class Interop
    {
        /// <summary>
        /// Gets a mod by its name.
        /// </summary>
        /// <param name="modName">The name of the mod to retrieve.</param>
        /// <returns>The mod with the specified name, or null if not found.</returns>
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

        /// <summary>
        /// Gets the assembly of a mod by its name.
        /// </summary>
        /// <param name="modName">The name of the mod to retrieve the assembly for.</param>
        /// <returns>The assembly of the mod with the specified name, or null if not found.</returns>
        public static Assembly GetModAssembly(string modName)
        {
            return GetModByName(modName).GetType().Assembly;
        }
	}
}
