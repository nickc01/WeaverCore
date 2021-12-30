using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Internal
{
	static class ModLoader
	{
		[OnRuntimeInit(int.MaxValue)]
		static void RuntimeInit()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					List<WeaverMod> weaverMods = new List<WeaverMod>();
					foreach (var type in assembly.GetTypes())
					{
						if (typeof(WeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
						{
							weaverMods.Add((WeaverMod)Activator.CreateInstance(type));
						}
					}
					foreach (var mod in weaverMods.OrderBy(m => m.LoadPriority()))
					{
						mod.Initialize();
						ReflectionUtilities.ExecuteMethodsWithAttribute<AfterModLoadAttribute>((_, a) => a.ModType.IsAssignableFrom(mod.GetType()));
					}
				}
				catch (Exception e)
				{
					WeaverLog.Log("Error Loading Mod [" + assembly.GetName().Name + " : " + e);
				}
			}
		}
	}
}
