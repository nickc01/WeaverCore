using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.TK2D;

namespace WeaverCore.Editor
{
	public static class TK2dCommon
	{
		public static bool OpenSpriteMap(out SpriteMapImport result)
		{
			return OpenSpriteMap(out result, out var _);
		}

		public static bool OpenSpriteMap(out SpriteMapImport result, out string path)
		{
			path = EditorUtility.OpenFilePanel("Open Sprite Map", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "spritemap");
			if (path.Length == 0)
			{
				result = null;
				return false;
			}
			result = JsonConvert.DeserializeObject<SpriteMapImport>(File.ReadAllText(path));
			return true;
		}

		public static bool OpenAnimationMap(out AnimationMapImport result)
		{
			return OpenAnimationMap(out result, out var _);
		}

		public static bool OpenAnimationMap(out AnimationMapImport result, out string path)
		{
			path = EditorUtility.OpenFilePanel("Open Animation Map", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "animmap");
			if (path.Length == 0)
			{
				result = null;
				return false;
			}
			result = JsonConvert.DeserializeObject<AnimationMapImport>(File.ReadAllText(path), new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects
			});
			return true;
		}

		public static bool OpenImage(string path, out Texture2D output)
		{
			var info = new FileInfo(path);
			Texture2D tex = new Texture2D(2, 2);
			tex.name = info.Name;
			try
			{
				ImageConversion.LoadImage(tex, File.ReadAllBytes(path));
			}
			catch (Exception e)
			{
				Debug.LogError($"Error Loading Image from file [{path}]");
				Debug.LogException(e);
				output = null;
				return false;
			}
			output = tex;
			return true;
		}

		public static bool OpenImage(out Texture2D output)
		{
			var path = EditorUtility.OpenFilePanelWithFilters("Open Sprite Map", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), new string[] { "Image files", "png,jpg,jpeg", "All files", "*" });
			if (path.Length == 0)
			{
				output = null;
				return false;
			}

			var info = new FileInfo(path);
			Texture2D tex = new Texture2D(2, 2);
			tex.name = info.Name;
			try
			{
				ImageConversion.LoadImage(tex, File.ReadAllBytes(path));
			}
			catch (Exception e)
			{
				Debug.LogError($"Error Loading Image from file [{path}]");
				Debug.LogException(e);
				output = null;
				return false;
			}
			output = tex;
			return true;
		}
	}
}
