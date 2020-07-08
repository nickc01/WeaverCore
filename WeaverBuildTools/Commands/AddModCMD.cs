using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WeaverBuildTools.Commands
{
	/*public static class AddModCMD
	{
		public static void AddMod(string assemblyToAddTo, string typeNamespace, string typeName, string modName, bool unloadable, string hollowKnightEXEPath, string weaverCorePath)
		{
			string hollowKnightAssemblyPath = hollowKnightEXEPath + @"hollow_knight_Data\Managed\Assembly-CSharp.dll";

			if (!File.Exists(hollowKnightAssemblyPath))
			{
				throw new FileNotFoundException("Unable to find Hollow Knight's Assembly-CSharp.dll. Make sure the hollow knight directory is set correctly.");
			}

			string coreModulePath = hollowKnightEXEPath + @"hollow_knight_Data\Managed\UnityEngine.CoreModule.dll";
			using (var patcher = new ModPatcher(assemblyToAddTo, hollowKnightAssemblyPath, weaverCorePath, coreModulePath))
			{
				string newNamespace = typeNamespace;
				if (newNamespace != null && newNamespace != "")
				{
					newNamespace = ".";
				}
				patcher.Patch(newNamespace + "_WeaverMod_", typeName, newNamespace + typeName, modName);
			}
		}
	}*/
}
