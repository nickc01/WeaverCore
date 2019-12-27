using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ViridianCore.Helpers
{
    public static class Methods
    {
        public static R GetFunction<R>(MethodInfo method) where R : Delegate
        {
            return (R)Delegate.CreateDelegate(typeof(R), method);
        }

        public static R GetFunction<R>(MethodInfo method,object instance) where R : Delegate
        {
            return (R)Delegate.CreateDelegate(typeof(R),instance, method);
        }

        public static R GetFunction<R,T>(string methodName,BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) where R : Delegate
        {
            return GetFunction<R>(typeof(T).GetMethod(methodName,Flags));
        }

        public static R GetFunction<R, T>(string methodName,T instance, BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) where R : Delegate
        {
            return (R)Delegate.CreateDelegate(typeof(R), instance, typeof(T).GetMethod(methodName, Flags));
        }
    }
}
