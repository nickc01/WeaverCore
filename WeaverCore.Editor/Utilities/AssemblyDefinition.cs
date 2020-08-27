using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	[Serializable]
	public class AssemblyDefinition
	{
		public string name;
		public List<string> references;
		public List<string> includePlatforms;
		public List<string> excludePlatforms;


		public static AssemblyDefinition Load(string path)
		{
			if (path.StartsWith("Assets"))
			{
				path = new FileInfo(path).FullName;
			}
			return JsonUtility.FromJson<AssemblyDefinition>(File.ReadAllText(path));
		}

		public static AssemblyDefinition Load(FileInfo path)
		{
			return Load(path.FullName);
		}

		public static void Save(AssemblyDefinition def,string path)
		{
			if (path.StartsWith("Assets"))
			{
				path = new FileInfo(path).FullName;
			}
			File.WriteAllText(path,JsonUtility.ToJson(def));
		}
	}
}
