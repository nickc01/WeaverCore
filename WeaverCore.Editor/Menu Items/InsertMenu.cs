using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Menu_Items
{

    public static class InsertMenu
	{
		static GameObject InsertObject(string prefabName)
		{
			var prefab = WeaverAssets.LoadWeaverAsset<GameObject>(prefabName);
			if (prefab == null)
			{
				prefab = EditorAssets.LoadEditorAsset<GameObject>(prefabName);
			}

			if (prefab == null)
			{
				return null;
			}

			var instance = GameObject.Instantiate(prefab, null);

			instance.name = prefab.name;

			if (!Application.isPlaying)
			{
				EditorSceneManager.SaveScene(instance.scene);
			}

			return instance;
		}

		[MenuItem("WeaverCore/Insert/Demo Player")]
		public static void InsertDemoPlayer()
		{
			InsertObject("Demo Player");
		}

		[MenuItem("WeaverCore/Insert/Blur Plane")]
		public static void InsertBlurPlane()
		{
			InsertObject("BlurPlane");
		}

		[MenuItem("WeaverCore/Insert/Game Cameras")]
		public static void InsertGameCameras()
		{
			InsertObject("Game Cameras");
		}

		[MenuItem("WeaverCore/Insert/Game Manager")]
		public static void InsertGameManager()
		{
			InsertObject("GameManager");
		}

		[MenuItem("WeaverCore/Insert/Weaver Canvas")]
		public static void InsertWeaverCanvas()
		{
			//var prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Canvas");

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

			InsertObject("Weaver Canvas");

			/*var instance = GameObject.Instantiate(prefab, null);

			instance.name = prefab.name;

			if (!Application.isPlaying)
			{
				EditorSceneManager.SaveScene(instance.scene);
			}*/

		}
	}
}
