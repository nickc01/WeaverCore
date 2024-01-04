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

        /// <summary>
        /// Checks if an object is truly null (Completely deallocated)
        /// </summary>
        /// <param name="obj">The object to check</param>
        public static bool IsObjectTrulyNull(UnityEngine.Object obj)
        {
            return ((object)obj) == null;
        }
        
        /// <summary>
        /// Checks if an object is still alive
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>Returns true if the object is still alive, and false if destroyed</returns>
        public static bool ObjectIsAlive(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is UnityEngine.Object unityObj)
            {
                return UnityUtilities.ObjectIsAlive(unityObj);
            }
            else
            {
                return false;
            }
        }
    }
}
