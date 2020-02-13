using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ViridianLink.Implementations;
using Harmony;
using ViridianLink.Extras;

namespace ViridianLink.Game
{
    public class Initializer : InitializerImplementation
    {
        public override void Initialize()
        {
            ApplyPatches();
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
