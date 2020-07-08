using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace WeaverCore.Editor
{
	public static class Reloader
	{
		[MenuItem("WeaverCore/Reload %F6")]
		public static void Reload()
		{
			AssetDatabase.Refresh();
		}
	}
}
