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
				Undo.RegisterCreatedObjectUndo(instance.gameObject, $"Create {prefabName}");
				//EditorSceneManager.SaveScene(instance.scene);
				EditorSceneManager.MarkSceneDirty(instance.scene);
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

		[MenuItem("WeaverCore/Insert/Weaver NPC")]
		public static void InsertWeaverNPC()
		{
			InsertObject("Weaver NPC");
		}

		[MenuItem("WeaverCore/Insert/Weaver Bench")]
		public static void InsertWeaverBench()
		{
			InsertObject("Weaver Bench");
		}

		[MenuItem("WeaverCore/Insert/Weaver Scene Manager")]
		public static void InsertWeaverSceneManager()
		{
			InsertObject("Weaver Scene Manager");
		}

		[MenuItem("WeaverCore/Insert/Transition Point")]
		public static void InsertWeaverTransitionPoint()
		{
			InsertObject("Transition Point Template");
		}

		[MenuItem("WeaverCore/Insert/Dreamnail Warp Object")]
		public static void InsertDreamWarpObject()
		{
			InsertObject("Dream Warp Object");
		}

		[MenuItem("WeaverCore/Insert/Weaver Canvas")]
		public static void InsertWeaverCanvas()
		{
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
		}
	}
}
