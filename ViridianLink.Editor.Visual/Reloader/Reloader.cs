using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace ViridianLink.Editor.Visual
{
	public static class Reloader
	{
		[MenuItem("ViridianLink/Reload %F6")]
		public static void Reload()
		{
			AssetDatabase.Refresh();
		}
	}
}
