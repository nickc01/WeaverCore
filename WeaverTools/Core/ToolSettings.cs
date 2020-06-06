using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeaverCore;
using WeaverCore.Helpers;

namespace WeaverTools
{
	public struct IDPair
	{
		public long FileID;
		public long PathID;
		public string Type;
	}

	public class ToolSettings
	{


		static ToolSettings Cache;

		public bool DebugMode = true;
		public bool PrintUnityDebugLogs = false;
		public bool PrintIDs = false;

		public List<IDPair> IDsToPrint = new List<IDPair>();

		public static ToolSettings GetSettings()
		{
			if (Cache == null)
			{
				var dir = WeaverCore.WeaverDirectory.GetWeaverDirectory();
				var toolsDir = dir.CreateSubdirectory("Tools");

				var settingsFile = WeaverDirectory.TrailingSlash(toolsDir.FullName) + "Settings.json";

				if (File.Exists(settingsFile))
				{
					Cache = Json.Deserialize<ToolSettings>(File.ReadAllText(settingsFile));

					Debugger.Log("Enabled = " + Cache.DebugMode);
				}
				else
				{
					Cache = new ToolSettings();
					SetSettings(Cache);
				}
			}
			return Cache;
		}

		public static void SetSettings(ToolSettings settings)
		{
			Cache = settings;

			var dir = WeaverCore.WeaverDirectory.GetWeaverDirectory();
			var toolsDir = dir.CreateSubdirectory("Tools");

			var settingsFile = WeaverDirectory.TrailingSlash(toolsDir.FullName) + "Settings.json";

			using (var file = File.CreateText(settingsFile))
			{
				file.Write(Json.Serialize(settings));
			}
		}
	}
}
