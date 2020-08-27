using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class WeaverCanvasCreate
	{
		[MenuItem("WeaverCore/Create/Weaver Canvas")]
		public static void Create()
		{
			var prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas");

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

			var instance = GameObject.Instantiate(prefab, null);

			instance.name = prefab.name;

			EditorSceneManager.SaveScene(instance.scene);

		}
	}
}
