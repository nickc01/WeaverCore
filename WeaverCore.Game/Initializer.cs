using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using Harmony;
using WeaverCore.Helpers;
using WeaverCore.Internal;

namespace WeaverCore.Game
{
    public class Initializer : InitializerImplementation
    {
        public override void Initialize()
        {
            ApplyPatches();
            ModLoader.LoadAllMods();
        }


        static void ApplyPatches()
        {
            foreach (var type in typeof(IPatch).Assembly.GetTypes())
            {
                if (typeof(IPatch).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
                {
                    var patch = (IPatch)Activator.CreateInstance(type);

                    patch.Apply();
                }
            }
        }
    }
}
