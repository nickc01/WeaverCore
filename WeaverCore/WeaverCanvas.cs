
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// A component used to access and manipulate the UI canvas
	/// </summary>
	public class WeaverCanvas : MonoBehaviour
	{
		/*[OnHarmonyPatch]
		static void HarmonyPatch(HarmonyPatcher patcher)
        {
			var orig = typeof(Transform).GetProperty(nameof(Transform.position)).GetSetMethod();
			var patch = typeof(WeaverCanvas).GetMethod(nameof(Position_Prefix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			patcher.Patch(orig,patch,null);
        }

		static bool Position_Prefix(Transform __instance, Vector3 value)
        {

			//Transform __instance, Vector3 value
			if (__instance.gameObject.name == "tk2dCamera")
            {
				Debug.Log("New Camera Position = " + value);
            }
			return true;
        }*/

		static GameObject _hudBlanker;
		static GameObject _hudBlankerWhite;

		/// <summary>
		/// Gets the HUD blanker used to blank out the screen with a solid black color
		/// </summary>
		public static GameObject HUDBlanker
		{
			get
			{
				if (_hudBlanker == null)
				{
					_hudBlanker = GameObject.FindObjectOfType<HUDCamera>().transform.Find("Blanker")?.gameObject;
				}
				return _hudBlanker;
			}
		}

		/// <summary>
		/// Gets the HUD blanker used to blank out the screen with a solid white color
		/// </summary>
		public static GameObject HUDBlankerWhite
		{
			get
			{
				if (_hudBlankerWhite == null)
				{
					_hudBlankerWhite = GameObject.FindObjectOfType<HUDCamera>().transform.Find("Blanker White")?.gameObject;
				}
				return _hudBlankerWhite;
			}
		}

		[AfterCameraLoad(int.MinValue)]
		static void Init()
		{
			Instance = GameObject.FindObjectOfType<WeaverCanvas>();
			if (Instance == null)
			{
				var prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas");
				if (Initialization.Environment == WeaverCore.Enums.RunningState.Editor)
				{
					Instance = GameObject.Instantiate(prefab, null).GetComponent<WeaverCanvas>();
				}
				else
				{
					var uiCanvas = GameObject.FindGameObjectWithTag("UIManager").transform.Find("UICanvas");
					GameObject.Instantiate(prefab.transform.Find("CONTENT GOES HERE"), uiCanvas).name = "CONTENT GOES HERE";
					GameObject.Instantiate(prefab.transform.Find("SCENE CONTENT GOES HERE"), uiCanvas).name = "SCENE CONTENT GOES HERE";
					uiCanvas.gameObject.AddComponent<WeaverCanvas>();
				}
			}
		}

		/// <summary>
		/// Gets the current UI canvas in the game
		/// </summary>
		public static WeaverCanvas Instance { get; private set; }

		/// <summary>
		/// The child object where UI content should be placed
		/// </summary>
		public static Transform Content
		{
			get
			{
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
				return Instance.transform.Find("SCENE CONTENT GOES HERE");
			}
		}

		static bool hookAdded = false;

		void Awake()
		{
			Instance = this;
			if (!hookAdded)
			{
				hookAdded = true;
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
			}
			GameObject.DontDestroyOnLoad(gameObject);
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

			if (Initialization.Environment == WeaverCore.Enums.RunningState.Editor)
			{
				gameObject.name = "UICanvas";
				gameObject.transform.localPosition = new Vector3(0f, 0f, -18.1f);
				var canvas = gameObject.GetComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.scaleFactor = 0.9787037f;
				canvas.referencePixelsPerUnit = 64f;
				canvas.planeDistance = 20f;
				canvas.sortingLayerName = "HUD";
				canvas.sortingOrder = 1;
				canvas.normalizedSortingGridSize = 0.1f;
				Debug.Log(WeaverCamera.Instance);
				//WeaverCamera.Instance.
				//canvas.worldCamera = GameObject.FindObjectOfType<HUDCamera>().GetComponent<Camera>();
				canvas.worldCamera = WeaverCamera.Instance.Cameras.hudCamera;

			}

			StartCoroutine(Initializer());
		}

		static List<CanvasExtension> featuresToAdd;

		[OnFeatureLoad]
		static void OnFeatureLoad(CanvasExtension feature)
		{
			if (Instance == null)
			{
				if (featuresToAdd == null)
				{
					featuresToAdd = new List<CanvasExtension>();
				}
				featuresToAdd.Add(feature);
			}
			else
			{
				feature.AddToWeaverCanvas();
			}
		}

		[OnFeatureUnload]
		static void OnFeatureUnload(CanvasExtension feature)
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
							extension.AddToWeaverCanvas();
						}
#else
					extension.AddToWeaverCanvas();
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