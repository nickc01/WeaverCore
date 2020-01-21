using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ViridianCore.Helpers
{
    public struct HarmonyInstance
    {
        internal object harmonyInstance { get; set; }

        internal HarmonyInstance(object HarmonyInstance)
        {
            harmonyInstance = HarmonyInstance;
        }

        public override bool Equals(object obj)
        {
            return obj is HarmonyInstance instance && instance.harmonyInstance == harmonyInstance;
        }
        public override int GetHashCode()
        {
            return harmonyInstance.GetHashCode();
        }
        public override string ToString()
        {
            return harmonyInstance.ToString();
        }

        public static bool operator ==(HarmonyInstance left, HarmonyInstance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HarmonyInstance left, HarmonyInstance right)
        {
            return !(left == right);
        }

        public DynamicMethod Patch(MethodInfo original, MethodInfo prefix, MethodInfo postfix)
        {
            return Harmony.Patch(this, original, prefix, postfix);
        }

        public void PatchAll(Assembly assembly = null)
        {
            Harmony.PatchAll(this, assembly);
        }
    }

    public static class Harmony
    {
        static Assembly HarmonyAssembly;
        static Type HarmonyInstanceType;
        static Type HarmonyMethodType;

        static MethodInfo CreateInstanceMethod;
        static MethodInfo PatchMethod;
        static MethodInfo PatchAllMethod;

        static HarmonyInstance harmonyInstance;

        internal static HarmonyInstance ViridianHarmonyInstance
        {
            get
            {
                return harmonyInstance;
            }
        }

        static Harmony()
        {
            if (HarmonyAssembly == null)
            {
                HarmonyAssembly = Assembly.Load("0Harmony");
                HarmonyInstanceType = HarmonyAssembly.GetType("Harmony.HarmonyInstance");
                HarmonyMethodType = HarmonyAssembly.GetType("Harmony.HarmonyMethod");

                PatchMethod = HarmonyInstanceType.GetMethod("Patch", BindingFlags.Instance | BindingFlags.Public);

                CreateInstanceMethod = HarmonyInstanceType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                harmonyInstance = Create("com." + nameof(ViridianCore).ToLower() + ".nickc01");
                PatchAllMethod = HarmonyInstanceType.GetMethod("PatchAll", new Type[] { typeof(Assembly) });
            }
        }

        public static HarmonyInstance Create(string id)
        {
            return new HarmonyInstance(CreateInstanceMethod.Invoke(null, new object[] { id }));
        }

        public static DynamicMethod Patch(HarmonyInstance instance, MethodInfo original, MethodInfo prefix, MethodInfo postfix)
        {
            var prefixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { prefix });
            var postfixInstance = Activator.CreateInstance(HarmonyMethodType, new object[] { postfix });
            return (DynamicMethod)PatchMethod.Invoke(instance.harmonyInstance, new object[] { original, prefixInstance, postfixInstance, null });
        }

        public static void PatchAll(HarmonyInstance instance, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }
            PatchAllMethod.Invoke(instance.harmonyInstance, new object[] { assembly });
        }
    }
}
