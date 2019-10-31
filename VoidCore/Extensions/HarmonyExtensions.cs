using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VoidCore
{
    public static class HarmonyExtensions
    {
        public static void PatchAllSafe(this HarmonyInstance instance, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            foreach (var type in assembly.GetTypesSafe())
            {
                var methods = HarmonyMethodExtensions.GetHarmonyMethods(type);
                if (methods != null && methods.Count > 0)
                {
                    HarmonyMethod attributes = HarmonyMethod.Merge(methods);
                    PatchProcessor patchProcessor = new PatchProcessor(instance, type, attributes);
                    patchProcessor.Patch();
                }
            }
        }
    }
}
