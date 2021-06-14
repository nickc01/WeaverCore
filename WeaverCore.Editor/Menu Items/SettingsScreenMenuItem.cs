using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace WeaverCore.Editor.Settings
{
	public class SettingsScreenMenuItem
	{
		[MenuItem("WeaverCore/Settings/General Settings")]
		public static void OpenGeneralSettingsMenu()
		{
			GeneralSettingsScreen.OpenSettingsScreen();
		}
	}
}
