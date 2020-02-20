using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Helpers
{
    public static class UnityHelpers
    {
        public static Func<UnityEngine.Object, bool> IsNativeObjectAlive { get; private set; }// = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");

        //public static Func<UnityEngine.Object, bool> IsNativeObjectAlive; // = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");
        public static Func<UnityEngine.Object, IntPtr> GetCachedPtr { get; private set; } = Methods.GetFunction<Func<UnityEngine.Object, IntPtr>, UnityEngine.Object>("GetCachedPtr");

        static UnityHelpers()
        {
            IsNativeObjectAlive = o =>
            {
                return GetCachedPtr(o) != IntPtr.Zero;
            };
        }
    }
}
