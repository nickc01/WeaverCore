using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Represents a built asset bundle file
	/// </summary>
	public struct BuiltAssetBundle
	{
		/// <summary>
		/// The file location of the asset bundle
		/// </summary>
		public FileInfo File;

		/// <summary>
		/// The target platform the asset bundle was built against
		/// </summary>
		public BuildTarget Target;
	}
}
