using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Utilities
{
    /// <summary>
	/// Contains many utility functions for handling methods and functions
	/// </summary>
    public static class MethodUtilities
    {
        /// <summary>
        /// Converts a methodInfo object into a callable delegate
        /// </summary>
        /// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
        /// <param name="method">The method info</param>
        /// <returns>The delegate that will call the method</returns>
        public static DelegateType ConvertToDelegate<DelegateType>(MethodInfo method) where DelegateType : Delegate
        {
            return (DelegateType)Delegate.CreateDelegate(typeof(DelegateType), method);
        }

        /// <summary>
        /// Converts a methodInfo object into a callable delegate
        /// </summary>
        /// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
        /// <param name="method">The method info</param>
        /// <param name="instance">The object instance that the method is bound to</param>
        /// <returns>The delegate that will call the method</returns>
        public static DelegateType ConvertToDelegate<DelegateType>(MethodInfo method, object instance) where DelegateType : Delegate
        {
            return (DelegateType)Delegate.CreateDelegate(typeof(DelegateType), instance, method);
        }

        /// <summary>
        /// Converts a methodInfo object into a callable delegate
        /// </summary>
        /// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
        /// <typeparam name="InstanceType">The instance type the method is bound to</typeparam>
        /// <param name="methodName">The name of the method</param>
        /// <param name="Flags">The binding flags used to find the method on the object</param>
        /// <returns>The delegate that will call the method</returns>
        public static DelegateType ConvertToDelegate<DelegateType, InstanceType>(string methodName, BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) where DelegateType : Delegate
        {
            return ConvertToDelegate<DelegateType>(typeof(InstanceType).GetMethod(methodName, Flags));
        }

        /// <summary>
        /// Converts a methodInfo object into a callable delegate
        /// </summary>
        /// <typeparam name="DelegateType">The delegate type to convert type</typeparam>
        /// <typeparam name="InstanceType">The instance type the method is bound to</typeparam>
        /// <param name="methodName">The name of the method</param>
        /// <param name="instance">The instance object the method is bound to</param>
        /// <param name="Flags">The binding flags used to find the method on the object</param>
        /// <returns>The delegate that will call the method</returns>
        public static DelegateType ConvertToDelegate<DelegateType, InstanceType>(string methodName, InstanceType instance, BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) where DelegateType : Delegate
        {
            return (DelegateType)Delegate.CreateDelegate(typeof(DelegateType), instance, typeof(InstanceType).GetMethod(methodName, Flags));
        }

        public static MethodInfo GetMethod<InstanceType>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            return typeof(InstanceType).GetMethod(methodName, flags);
        }

        /*public static MethodInfo GetMethod(string typePath)
        {
            if (typePath.Length < 3)
            {
                throw new Exception("Invalid Type Path format. Format should be {Namespace}.{Class}.{Method}");
            }
            var lastIndex = typePath.LastIndexOf(".");
            if (lastIndex == 0 || lastIndex == -1 || lastIndex == typePath.Length - 1)
            {
                throw new Exception("Invalid Type Path format. Format should be {Namespace}.{Class}.{Method}");
            }

            var methodName = typePath.Substring(lastIndex + 1);
            var className = typePath.Remove(lastIndex);

            Type classType = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                classType = assembly.GetType(className);
                if (classType != null)
                {
                    break;
                }
            }

            if (classType == null)
            {
                throw new Exception($"Unable to find class {className}");
            }

            var method = classType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                throw new Exception($"Unable to find method {methodName} of class {className}");
            }

            return method;
        }*/
    }
}