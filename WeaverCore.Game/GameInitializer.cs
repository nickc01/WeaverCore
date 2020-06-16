using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using Harmony;
using WeaverCore.Utilities;
using WeaverCore.Internal;
using HutongGames.PlayMaker.Actions;
using WeaverCore.Game.Implementations;
using WeaverCore.Interfaces;
using System.Reflection;
using System.Linq;

namespace WeaverCore.Game
{
    class GameInitializer : IInit
    {
        void IInit.OnInit()
        {
            ModLoader.LoadAllMods();
			IRuntimePatchRunner.RuntimeInit();
        }

		class IRuntimePatchRunner
		{
			public static void RuntimeInit()
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					DoRuntimeInit(assembly);
				}

				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}

			private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
			{
				DoRuntimeInit(args.LoadedAssembly);
			}

			static void DoRuntimeInit(Assembly assembly)
			{
				foreach (var type in assembly.GetTypes().Where(t => typeof(IRuntimeInit).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
				{
					var rInit = (IRuntimeInit)Activator.CreateInstance(type);
					try
					{
						rInit.RuntimeInit();
					}
					catch (Exception e)
					{
						Debugger.LogError("Runtime Init Error: " + e);
					}
				}
			}
		}



		/*static void ApplyPatches()
        {
            foreach (var type in typeof(IPatch).Assembly.GetTypes())
            {
                if (typeof(IPatch).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
                {
                    var patch = (IPatch)Activator.CreateInstance(type);

                    patch.Apply();
                }
            }
        }*/
	}
}
