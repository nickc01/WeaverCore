using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
	public struct BuiltAssetBundle
	{
		public FileInfo File;
		public BuildTarget Target;
	}
}
