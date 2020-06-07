using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using System.Reflection;
using System.Linq;
using WeaverCore.Helpers;
using static WeaverCore.Helpers.Harmony;
using WeaverCore.Internal.Harmony;

namespace WeaverCore.Internal
{
    internal static class ModuleInitializer
    {
        static bool Initialized = false;

        internal static HarmonyInstance weaverCorePatcher;

        public static void Initialize()
        {
            if (!Initialized)
            {
				weaverCorePatcher = Create($"com.{(nameof(WeaverCore).ToLower())}.nickc01");
                Initialized = true;
                PatchGetTypes();


                var init = ImplFinder.GetImplementation<Initializer_I>();
                init.Initialize();

                GameObjectPatches.Patch(weaverCorePatcher);
            }
        }

        static void PatchGetTypes()
        {
            var method = typeof(Assembly).GetMethod("GetTypes");

            weaverCorePatcher.Patch(method, null, typeof(ModuleInitializer).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic));
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
}
