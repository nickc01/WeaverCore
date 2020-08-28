
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using WeaverCore;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class WeaverCanvas : MonoBehaviour 
{
	class OnGameStart : IRuntimeInit
	{
		void IRuntimeInit.RuntimeInit()
		{
			Debug.Log("Spawning Canvas");
			if (GameObject.FindObjectOfType<WeaverCanvas>() == null)
			{
				GameObject.Instantiate(WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas"),null);
			}
		}
	}

	public static WeaverCanvas Instance { get; private set; }

	public static Transform Content
	{
		get
		{
			return Instance.transform.GetChild(0);
		}
	}

	void Awake()
	{
		Instance = this;
		GameObject.DontDestroyOnLoad(gameObject);
		Debug.Log("Init canvas");
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

	IEnumerator Initializer()
	{
		yield return null;

		var content = Instance.transform.GetChild(0);

		foreach (var extension in Registry.GetAllFeatures<CanvasExtension>())
		{
			if (extension.AddedByDefault)
			{
				if (!ContainedInObject(content.gameObject, extension.gameObject))
				{
					GameObject.Instantiate(extension.gameObject, content);
				}
			}
		}
	}

	static bool ContainedInObject(GameObject searchIn, GameObject childToSearchFor)
	{
		//var childTransform = childToSearchFor.transform;
		//var childhash = TransformUtilities.GetHashOfTransform(childToSearchFor.transform);
		for (int i = 0; i < searchIn.transform.childCount; i++)
		{
			if (searchIn.transform.GetChild(i).gameObject.name == childToSearchFor.name)
			{
				return true;
			}



			/*var hash = TransformUtilities.GetHashOfTransform(searchIn.transform.GetChild(i));
			if (hash == childhash)
			{
				return true;
			}*/
		}

		return false;
	}
}
