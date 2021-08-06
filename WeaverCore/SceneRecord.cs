using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WeaverCore
{

	/// <summary>
	/// Contains a list of scenes a mod is adding to the game
	/// </summary>
	[ShowFeature]
	[CreateAssetMenu(fileName = "Scene Record", menuName = "WeaverCore/Scene Record")]
	public class SceneRecord : ScriptableObject
	{
#if UNITY_EDITOR
		[SerializeField]
		List<UnityEditor.SceneAsset> sceneAssets = new List<SceneAsset>();
#endif

		[SerializeField]
#if UNITY_EDITOR
		[HideInInspector]
#endif
		List<string> scenePaths = new List<string>();


		public IEnumerable<string> Scenes
		{
			get
			{
				return scenePaths;
			}
		}



#if UNITY_EDITOR
		private void OnValidate()
		{
			if (scenePaths.Count != sceneAssets.Count)
			{
				scenePaths = new List<string>();
				foreach (var asset in sceneAssets)
				{
					scenePaths.Add(AssetDatabase.GetAssetPath(asset));
				}
			}
			else
			{
				for (int i = 0; i < sceneAssets.Count; i++)
				{
					var path = AssetDatabase.GetAssetPath(sceneAssets[i]);
					if (scenePaths[i] != path)
					{
						scenePaths[i] = path;
					}
				}
			}
		}

		[BeforeBuild]
		static void SetBundles()
		{
			Debug.Log("Before Build");
			var scenes = AssetDatabase.FindAssets($"t:{nameof(SceneRecord)}");
			foreach (var guid in scenes)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var record = AssetDatabase.LoadAssetAtPath<SceneRecord>(path);
				if (record != null)
				{
					var oldBundleName = GetAssetBundleName(record);
					var match = Regex.Match(oldBundleName, @"([\d\w]+?)_bundle");
					if (match.Success)
					{
						var newBundleName = $"{match.Groups[1].Value}_scenes_bundle";
						foreach (var sceneAsset in record.sceneAssets)
						{
							SetAssetBundleName(newBundleName, sceneAsset);
						}
					}
				}
			}
		}

		static void SetAssetBundleName(string bundleName, UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				import.SetAssetBundleNameAndVariant(bundleName, import.assetBundleVariant);
			}
		}

		static string GetAssetBundleName(UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				return import.assetBundleName;
			}
			return "";
		}

#endif
	}
}
