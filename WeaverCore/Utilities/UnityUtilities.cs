using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to unity objects
    /// </summary>
    public static class UnityUtilities
    {
        /// <summary>
        /// Checks if the native pointer for an object is still alive
        /// </summary>
        public static Func<UnityEngine.Object, bool> IsNativeObjectAlive { get; private set; }

        /// <summary>
        /// Gets the cached pointer for an object
        /// </summary>
        public static Func<UnityEngine.Object, IntPtr> GetCachedPtr { get; private set; }

        static UnityUtilities()
        {
            GetCachedPtr = ReflectionUtilities.MethodToDelegate<Func<UnityEngine.Object, IntPtr>, UnityEngine.Object>("GetCachedPtr");
            IsNativeObjectAlive = o =>
            {
                return GetCachedPtr(o) != IntPtr.Zero;
            };
        }

        public static bool IsObjectTrulyNull(UnityEngine.Object obj)
        {
            return ((object)obj) == null;
        }
    }
}
