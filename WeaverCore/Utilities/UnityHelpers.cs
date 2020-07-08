using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
    public static class UnityHelpers
    {
        public static Func<UnityEngine.Object, bool> IsNativeObjectAlive { get; private set; }// = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");

        //public static Func<UnityEngine.Object, bool> IsNativeObjectAlive; // = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");
        public static Func<UnityEngine.Object, IntPtr> GetCachedPtr { get; private set; }

        static UnityHelpers()
        {
            GetCachedPtr = MethodUtilities.ConvertToDelegate<Func<UnityEngine.Object, IntPtr>, UnityEngine.Object>("GetCachedPtr");
            IsNativeObjectAlive = o =>
            {
                return GetCachedPtr(o) != IntPtr.Zero;
            };
        }
    }
}
