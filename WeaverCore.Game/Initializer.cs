using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using Harmony;
using WeaverCore.Helpers;
using WeaverCore.Internal;
using HutongGames.PlayMaker.Actions;
using WeaverCore.Game.Implementations;

namespace WeaverCore.Game
{
    public class Initializer : Initializer_I
    {
        public override void Initialize()
        {
            ApplyPatches();
            ModLoader.LoadAllMods();
            new Implementations.G_RuntimeInitializer_I().Awake();

            G_URoutine_I.BeginExecuter();
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
