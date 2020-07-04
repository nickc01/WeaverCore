using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using System.Reflection;
using System.Linq;
using WeaverCore.Utilities;
//
using WeaverCore.Internal.Harmony;
using WeaverCore.Interfaces;

namespace WeaverCore.Internal
{
    static class ModuleInitializer
    {
        static bool Initialized = false;

        public static void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                ImplFinder.Init();
                InitRunner.RunInitFunctions();
            }
        }

        /*class GetTypesPatch : IPatch
        {
            //TODO TODO TODO - WORk on IInit and IPatch classes
            public void Patch(HarmonyPatcher patcher)
            {
                patcher.Patch(typeof(Assembly).GetMethod("GetTypes"),null, typeof(ModuleInitializer).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic));
            }
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
        }*/


    }
}
