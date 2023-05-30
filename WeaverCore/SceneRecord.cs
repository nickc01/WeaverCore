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


#if UNITY_EDITOR
    [Serializable]
	struct SceneReplacement
	{
		[Tooltip("The name of the scene to replace in-game")]
		public string SceneToReplace;
		[Tooltip("The scene to replace it with")]
		public UnityEditor.SceneAsset Replacement;
	}

	[Serializable]
	struct SceneUnion
	{
		[Tooltip("The name of the scene to combine with in-game")]
		public string SceneToUniteWith;
		[Tooltip("The scene to combine to. This scene will be loaded when \"Scene To Unite With\" is also loaded, combining the contents of the two scenes together")]
		public UnityEditor.SceneAsset UnionScene;
	}
#endif

	/// <summary>
	/// Contains a list of scenes a mod is adding to the game
	/// </summary>
	[ShowFeature]
	[CreateAssetMenu(fileName = "Scene Record", menuName = "WeaverCore/Scene Record")]
	public class SceneRecord : ScriptableObject
#if UNITY_EDITOR
		,ISerializationCallbackReceiver //Make the class inhertit from ISerializationCallbackReceiver if in the Editor Enviroment
#endif
	{
#if UNITY_EDITOR
		[SerializeField]
		[Tooltip("A list of scenes to add to the game")]
		List<UnityEditor.SceneAsset> sceneAdditions = new List<SceneAsset>();

		[SerializeField]
		[Tooltip("A list of scenes to replace in the game")]
		List<SceneReplacement> sceneReplacements = new List<SceneReplacement>();

		[SerializeField]
		[Tooltip("A list of scenes to combine with in game")]
		List<SceneUnion> sceneUnions = new List<SceneUnion>();

		[SerializeField]
		[Tooltip("A list of all the gates that are being changed to point to new destinations")]
		List<GateRedirect> transitionRedirects = new List<GateRedirect>();
#endif

		/// <summary>
		/// Contains all the information needed to change an existing Transition Point to point to a new scene
		/// </summary>
		[Serializable]
		public class GateRedirect
        {
			[Tooltip("The name of the scene that contains the gate that's getting redirected")]
			public string GateScene;
			[Tooltip("The name of the gate to redirect")]
			public string GateToChange;
			[Tooltip("The new scene the gate will be directed to when the player touches it")]
			public string NewScene;
			[Tooltip("The new gate the player will be placed at when the player arrives at the new scene")]
			public string NewGate;
        }

		[Space(40f)]
		[HideInInspector]
		[SerializeField]
		List<string> sceneAdditionPaths = new List<string>();


		[HideInInspector]
		[SerializeField]
		List<string> sceneToReplacePaths = new List<string>();
		[HideInInspector]
		[SerializeField]
		List<string> sceneReplacementPaths = new List<string>();

		[HideInInspector]
		[SerializeField]
		List<string> sceneToUnionizePaths = new List<string>();
		[HideInInspector]
		[SerializeField]
		List<string> sceneUnionPaths = new List<string>();


		[HideInInspector]
		[SerializeField]
		List<string> transitionRedirects_containingScenes = new List<string>();
		[HideInInspector]
		[SerializeField]
		List<string> transitionRedirects_gatesToRedirect = new List<string>();
		[HideInInspector]
		[SerializeField]
		List<string> transitionRedirects_newDestinationScenes = new List<string>();
		[HideInInspector]
		[SerializeField]
		List<string> transitionRedirects_newDestinationGateNames = new List<string>();


		/// <summary>
		/// A list of all the new scenes getting added to the game
		/// </summary>
		public IEnumerable<string> SceneAdditions => sceneAdditionPaths;

		/// <summary>
		/// A list of all the scenes that are getting replaced with new ones
		/// </summary>
		public IEnumerable<(string SceneToReplace,string Replacement)> SceneReplacements
		{
			get
			{
				for (int i = 0; i < sceneToReplacePaths.Count; i++)
				{
					yield return (sceneToReplacePaths[i],sceneReplacementPaths[i]);
				}
			}
		}

		/// <summary>
		/// A list of all the scenes that are being combined with new ones
		/// </summary>
		public IEnumerable<(string SceneToUnionize, string SceneUnion)> SceneUnions
		{
			get
			{
				for (int i = 0; i < sceneToUnionizePaths.Count; i++)
				{
					yield return (sceneToUnionizePaths[i], sceneUnionPaths[i]);
				}
			}
		}

		public IEnumerable<GateRedirect> TransitionRedirects
        {
			get
            {
                for (int i = 0; i < transitionRedirects_containingScenes.Count; i++)
                {
					yield return new GateRedirect
					{
						GateScene = transitionRedirects_containingScenes[i],
						NewGate = transitionRedirects_newDestinationGateNames[i],
						GateToChange = transitionRedirects_gatesToRedirect[i],
						NewScene = transitionRedirects_newDestinationScenes[i]
					};
                }
            }
        }

		/// <summary>
		/// Replaces the <paramref name="oldScene"/> with the <paramref name="newScene"/>. Anytime the old scene gets loaded, the new scene will load instead
		/// </summary>
		/// <param name="oldScene">The scene to be replaced</param>
		/// <param name="newScene">The scene to replace the old one</param>
		/// <returns></returns>
		public bool AddSceneReplacement(string oldScene, string newScene)
		{
			for (int i = 0; i < sceneToReplacePaths.Count; i++)
			{
				if (sceneToReplacePaths[i] == oldScene && sceneReplacementPaths[i] == newScene)
				{
					return false;
				}
			}

			sceneToReplacePaths.Add(oldScene);
			sceneReplacementPaths.Add(newScene);

			return true;
		}

		/// <summary>
		/// Removes a scene replacement
		/// </summary>
		/// <param name="oldScene">The scene that was to be replaced</param>
		/// <param name="newScene">The scene that was replacing the old one</param>
		/// <returns></returns>
		public bool RemoveSceneReplacement(string oldScene, string newScene)
		{
			for (int i = 0; i < sceneToReplacePaths.Count; i++)
			{
				if (sceneToReplacePaths[i] == oldScene && sceneReplacementPaths[i] == newScene)
				{
					sceneToReplacePaths.RemoveAt(i);
					sceneReplacementPaths.RemoveAt(i);

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Combines the contents of <paramref name="oldScene"/> with <paramref name="newScene"/>. Any time the old scene is loaded, the new scene will also get loaded, combining the two scenes together
		/// </summary>
		/// <param name="oldScene">The scene to be combined with</param>
		/// <param name="newScene">The scene to combine with the old one</param>
		/// <returns></returns>
		public bool AddSceneUnion(string oldScene, string newScene)
		{
			for (int i = 0; i < sceneToUnionizePaths.Count; i++)
			{
				if (sceneToUnionizePaths[i] == oldScene && sceneUnionPaths[i] == newScene)
				{
					return false;
				}
			}

			sceneToUnionizePaths.Add(oldScene);
			sceneUnionPaths.Add(newScene);

			return true;
		}

		/// <summary>
		/// Removes a scene union
		/// </summary>
		/// <param name="oldScene">The scene that was to be combined with</param>
		/// <param name="newScene">The scene that was to combine with the old one</param>
		/// <returns></returns>
		public bool RemoveSceneUnion(string oldScene, string newScene)
		{
			for (int i = 0; i < sceneToUnionizePaths.Count; i++)
			{
				if (sceneToUnionizePaths[i] == oldScene && sceneUnionPaths[i] == newScene)
				{
					sceneToUnionizePaths.RemoveAt(i);
					sceneUnionPaths.RemoveAt(i);

					return true;
				}
			}

			return false;
		}

#if UNITY_EDITOR

		[BeforeBuild]
		static void SetBundles()
		{
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
						foreach (var sceneAsset in record.sceneAdditions)
						{
							SetAssetBundleName(newBundleName, sceneAsset);
						}

						foreach (var sceneAsset in record.sceneReplacements)
						{
							SetAssetBundleName(newBundleName, sceneAsset.Replacement);
						}

						foreach (var sceneAsset in record.sceneUnions)
						{
							SetAssetBundleName(newBundleName, sceneAsset.UnionScene);
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

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
			sceneAdditionPaths = sceneAdditions.Select(s => AssetDatabase.GetAssetPath(s)).ToList();

			sceneToReplacePaths = sceneReplacements.Select(s => s.SceneToReplace).ToList();
			sceneReplacementPaths = sceneReplacements.Select(s => AssetDatabase.GetAssetPath(s.Replacement)).ToList();

			sceneToUnionizePaths = sceneUnions.Select(s => s.SceneToUniteWith).ToList();
			sceneUnionPaths = sceneUnions.Select(s => AssetDatabase.GetAssetPath(s.UnionScene)).ToList();

			transitionRedirects_containingScenes = transitionRedirects.Select(s => s.GateScene).ToList();
			transitionRedirects_gatesToRedirect = transitionRedirects.Select(s => s.GateToChange).ToList();
			transitionRedirects_newDestinationGateNames = transitionRedirects.Select(s => s.NewGate).ToList();
			transitionRedirects_newDestinationScenes = transitionRedirects.Select(s => s.NewScene).ToList();
		}

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }

#endif
    }
}
