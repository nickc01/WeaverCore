
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

public class WeaverCanvas : MonoBehaviour 
{
	[AfterCameraLoad(int.MinValue)]
	static void Init()
	{
		Instance = GameObject.FindObjectOfType<WeaverCanvas>();
		if (Instance == null)
		{
			Instance = GameObject.Instantiate(WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas"), null).GetComponent<WeaverCanvas>();
		}
	}

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
