using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ViridianLink.Extras;
using ViridianLink.Implementations;
using System.Reflection;
using System.Linq;

namespace ViridianLink
{
    internal static class ModuleInitializer
    {
        static bool Initialized = false;

        public static void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                PatchGetTypes();
                var init = ImplInfo.GetImplementation<InitializerImplementation>();
                init.Initialize();
                Extras.Harmony.ViridianHarmonyInstance.PatchAll();
            }
        }

        static void PatchGetTypes()
        {
            var method = typeof(Assembly).GetMethod("GetTypes");

            Extras.Harmony.ViridianHarmonyInstance.Patch(method, null, typeof(ModuleInitializer).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic));
        }

        static void Postfix(ref Type[] __result)
        {
            var list = __result.ToList();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var type = list[i];
                if (type.FullName == typeof(InitializeOnLoad).FullName)
                {
                    list.RemoveAt(i);
                }
            }
            __result = list.ToArray();
        }
    }

    public class Test : MonoBehaviour
    {

    }
}
