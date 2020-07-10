using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Internal
{
	public static class WeaverReloadTools
	{
		static string ReloadStorageFileLocation = new FileInfo("Assets\\WeaverCore\\Hidden~\\ReloadStorage.txt").FullName;

		[Serializable]
		class ReloadInfo
		{
			public bool DoReload = true;
			public bool DoAutoReloading = true;

			public ReloadInfo(bool doReload, bool doAutoReloading)
			{
				DoReload = doReload;
				DoAutoReloading = doAutoReloading;
			}
		}


		static ReloadInfo Info
		{
			get
			{
				if (File.Exists(ReloadStorageFileLocation))
				{
					return JsonUtility.FromJson<ReloadInfo>(File.ReadAllText(ReloadStorageFileLocation));
				}
				else
				{
					return new ReloadInfo(true, true);
				}
			}
			set
			{
				using (var fileStream = File.CreateText(ReloadStorageFileLocation))
				{
					fileStream.Write(JsonUtility.ToJson(value));
				}
			}
		}

		public static bool DoReloadTools
		{
			get
			{
				return Info.DoReload;
			}
			set
			{
				var info = Info;
				if (value != info.DoReload)
				{
					info.DoReload = value;
					Info = info;
				}
			}
		}

		public static bool DoOnScriptReload
		{
			get
			{
				return Info.DoAutoReloading;
			}
			set
			{
				var info = Info;
				if (value != info.DoAutoReloading)
				{
					info.DoAutoReloading = value;
					Info = info;
				}
			}
		}

		class Init : IInit
		{
			public void OnInit()
			{
				EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
			}

			[UnityEditor.Callbacks.DidReloadScripts]
			static void OnReload()
			{
				if (DoOnScriptReload)
				{
					DoReloadTools = true;
				}
			}

			private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
			{
				if (obj == PlayModeStateChange.ExitingEditMode)
				{
					DoReloadTools = false;
				}
				else
				{
					DoReloadTools = true;
				}
			}
		}
	}
}
