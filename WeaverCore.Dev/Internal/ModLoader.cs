using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Internal
{
	class ModLoader : IRuntimeInit
	{
		public void RuntimeInit()
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
					WeaverLog.Log("Mod Load Error: " + e);
				}
			}
		}
	}
}
