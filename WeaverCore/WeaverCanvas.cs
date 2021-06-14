
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
	/*[AfterModLoad(typeof(WeaverCore.Internal.WeaverCore),priority:int.MinValue)]
	static void Init()
	{
		if (GameObject.FindObjectOfType<WeaverCanvas>() == null)
		{
			GameObject.Instantiate(WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas"), null);
		}
	}*/
	[AfterCameraLoad(int.MinValue)]
	static void Init()
	{
		if (GameObject.FindObjectOfType<WeaverCanvas>() == null)
		{
			GameObject.Instantiate(WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas"), null);
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
	/// Thie child object where scene specific UI Content should be placed. Any time a new scene is loaded, all child objects of this Transform will get destroyed
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

		foreach (var extension in Registry.GetAllFeatures<CanvasExtension>())
		{
			if (extension.AddedOnStartup)
			{
#if UNITY_EDITOR
				if (!ContainedInObject(content.gameObject, extension.gameObject))
				{
					//GameObject.Instantiate(extension.gameObject, content);
					extension.AddToWeaverCanvas();
				}
#else
				extension.AddToWeaverCanvas();
#endif
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
