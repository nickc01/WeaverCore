/*using System.Reflection;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Useful utilities for interacting with the SFCore mod
    /// </summary>
    public static class SFCoreUtilities
	{
#if UNITY_EDITOR
        static Assembly sfCoreAssembly;
        static string sfCoreLoadError;
#endif

        /// <summary>
        /// Returns the last error message when attempting to load SFCore
        /// </summary>
        public static string GetLoadError()
        {

        }

        /// <summary>
        /// Attempts to load the SFCore assembly. Returns null if it couldn't be found or there was an error
        /// </summary>
        public static Assembly GetSFCoreAssembly()
        {
            if (sfCoreAssembly == null)
            {
                sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
                if (sfCoreAssembly == null)
                {
                    sfCoreLoadError = "Attempting to add a charm without SFCore installed. Install SFCore to fix this issue";
                    return null;
                }
            }
            return sfCoreAssembly;
        }
    }
}*/
