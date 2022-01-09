using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// Similar to <see cref="WeaverCanvas"/>, but this canvas will appear on top of everything else, and is mainly used for debugging tools that need to be visible above all other UI elements
	/// </summary>
	public class WeaverDebugCanvas : MonoBehaviour
	{
		public static WeaverDebugCanvas Instance { get; private set; }

		/// <summary>
		/// The child object where UI content should be placed
		/// </summary>
		public static Transform Content
		{
			get
			{
				if (Instance == null)
				{
					Init();
				}
				return Instance.transform.Find("CONTENT GOES HERE");
			}
		}

		/// <summary>
		/// The child object where scene specific UI Content should be placed. Any time a new scene is loaded, all child objects of this Transform will get destroyed
		/// </summary>
		public static Transform SceneContent
		{
			get
			{
				if (Instance == null)
				{
					Init();
				}
				return Instance.transform.Find("SCENE CONTENT GOES HERE");
			}
		}

		static bool hookAdded = false;
		static List<DebugCanvasExtension> featuresToAdd;

		[AfterCameraLoad(int.MinValue)]
		static void Init()
		{
			if (Instance == null)
			{
				var prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Debug Canvas");
				Instance = GameObject.Instantiate(prefab, default, Quaternion.identity).GetComponent<WeaverDebugCanvas>();
			}
		}

		private void Awake()
		{
			Instance = this;

			DontDestroyOnLoad(gameObject);
			if (!hookAdded)
			{
				hookAdded = true;
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
			}

			if (GameObject.FindObjectOfType<EventSystem>() == null)
			{
				var eventObject = new GameObject("Event System");
				if (Application.isPlaying)
				{
					GameObject.DontDestroyOnLoad(eventObject);
				}
				eventObject.AddComponent<EventSystem>();
				eventObject.AddComponent<StandaloneInputModule>();
			}

			StartCoroutine(Initializer());
		}

		[OnFeatureLoad]
		static void OnFeatureLoad(DebugCanvasExtension feature)
		{
			if (Instance == null)
			{
				if (featuresToAdd == null)
				{
					featuresToAdd = new List<DebugCanvasExtension>();
				}
				featuresToAdd.Add(feature);
			}
			else
			{
				feature.AddToDebugCanvas();
			}
		}

		[OnFeatureUnload]
		static void OnFeatureUnload(DebugCanvasExtension feature)
		{
			if (Instance == null)
			{
				featuresToAdd.Remove(feature);
			}
			else
			{
				var components = Instance.transform.GetComponentsInChildren(feature.GetType());
				for (int i = components.GetLength(0) - 1; i >= 0; i--)
				{
					if (components[i] != null && components[i].transform.parent == WeaverCanvas.Content)
					{
						GameObject.Destroy(components[i].gameObject);
					}
				}
			}
		}

		private static void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
		{
			if (Instance != null)
			{
				for (int i = SceneContent.childCount - 1; i >= 0; i--)
				{
					var child = SceneContent.GetChild(i);
					if (child != null)
					{
						GameObject.Destroy(child.gameObject);
					}
				}
			}
		}

		IEnumerator Initializer()
		{
			yield return null;

			var content = Instance.transform.GetChild(0);

			if (featuresToAdd != null)
			{
				foreach (var extension in featuresToAdd)
				{
					if (extension.AddedOnStartup)
					{
#if UNITY_EDITOR
						if (!ContainedInObject(content.gameObject, extension.gameObject))
						{
							extension.AddToDebugCanvas();
						}
#else
						extension.AddToDebugCanvas();
#endif
					}
				}
			}
		}
#if UNITY_EDITOR
		static bool ContainedInObject(GameObject searchIn, GameObject childToSearchFor)
		{
			for (int i = 0; i < searchIn.transform.childCount; i++)
			{
				if (searchIn.transform.GetChild(i).gameObject.name == childToSearchFor.name)
				{
					return true;
				}
			}

			return false;
		}
#endif
	}

}