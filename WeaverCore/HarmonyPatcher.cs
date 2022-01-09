using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// Uses the HarmonyX library for patching any methods/properties you like.
    /// 
    /// Should only be used if necessary
    /// </summary>
    public struct HarmonyPatcher
    {
        static Assembly HarmonyAssembly;
        static Type HarmonyInstanceType;
        static Type HarmonyMethodType;
        static MethodInfo PatchMethod;

        static HarmonyPatcher()
        {
            if (HarmonyAssembly == null)
            {
#if UNITY_EDITOR
                HarmonyAssembly = Assembly.Load("0Harmony");
#else
                HarmonyAssembly = ResourceUtilities.LoadAssembly("0Harmony");
#endif
                HarmonyInstanceType = HarmonyAssembly.GetType("HarmonyLib.Harmony");
                HarmonyMethodType = HarmonyAssembly.GetType("HarmonyLib.HarmonyMethod");
                PatchMethod = HarmonyInstanceType.GetMethod("Patch", new Type[] { typeof(MethodBase), HarmonyMethodType, HarmonyMethodType, HarmonyMethodType, HarmonyMethodType });
            }
        }

        object harmonyInstance;

        HarmonyPatcher(object HarmonyInstance)
        {
            harmonyInstance = HarmonyInstance;
        }

        public override bool Equals(object obj)
        {
            return obj is HarmonyPatcher && ((HarmonyPatcher)obj).harmonyInstance == harmonyInstance;
        }
        public override int GetHashCode()
        {
            return harmonyInstance.GetHashCode();
        }
        public override string ToString()
        {
            return harmonyInstance.ToString();
        }

        public static bool operator ==(HarmonyPatcher left, HarmonyPatcher right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HarmonyPatcher left, HarmonyPatcher right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Creates a new HarmonyPatcher
        /// </summary>
        /// <param name="id">The id to create it with</param>
        /// <returns>Returns the newly created Harmony Patcher. Use the <see cref="Patch(MethodBase, MethodInfo, MethodInfo)"/> method to start patching methods</returns>
        public static HarmonyPatcher Create(string id)
        {
            var value = Activator.CreateInstance(HarmonyInstanceType, new object[] { id });
            return new HarmonyPatcher(value);
        }

        /// <summary>
        /// Patches a method with a prefix and postfix. The prefix is called before the original is called, and the postfix is called after the original method is called
        /// </summary>
        /// <param name="original">The original method to patch</param>
        /// <param name="prefix">The prefix method. The prefix MUST be static and return a boolean value. The returned boolean determines whether the original method should be run or not. If no prefix method is needed, this can be null</param>
        /// <param name="postfix">The postfix method. The postfix MUST be static and return void. If no postfix is needed, this can be null</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException">Throws if the original method is null</exception>
        public void Patch(MethodBase original, MethodInfo prefix, MethodInfo postfix)
        {
            if (harmonyInstance == null)
            {
                throw new Exception("This patcher is null");
            }
            if (prefix == null && postfix == null)
            {
                return;
            }
            if (original == null)
            {
                throw new ArgumentNullException("original", "Method is null");
            }

            object prefixInstance = null;
            if (prefix != null)
            {
                prefixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { prefix });
            }
            object postfixInstance = null;
            if (postfix != null)
            {
                postfixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { postfix });
            }

            PatchMethod.Invoke(harmonyInstance, new object[] { original, prefixInstance, postfixInstance, null, null });
        }
    }
}
