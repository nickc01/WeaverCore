using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

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
					foreach (var type in assembly.GetTypes())
					{
						if (typeof(WeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
						{
							var mod = (WeaverMod)Activator.CreateInstance(type);
							mod.Initialize();
						}
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
