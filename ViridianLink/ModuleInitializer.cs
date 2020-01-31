using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ViridianLink.Helpers;
using ViridianLink.Implementations;

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
                var init = ImplInfo.GetImplementation<InitializerImplementation>();
                init.Initialize();
            }
        }
    }
}
