using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public class WeaverCamera : MonoBehaviour
	{
#if UNITY_EDITOR
		//This is for editor only. The game only version can be found in the TK2DCameraPatch class in the WeaverCore.Game project
		[OnRuntimeInit(int.MinValue)]
		static void EditorInit()
		{
			if (_instance != null)
			{
				return;
			}
			//Debug.Log("RUNTIME INIT CAMERA");
			Vector3 position = default;
			var mainCam = GameObject.FindObjectOfType<Camera>();
			if (mainCam != null)
			{
				position = mainCam.transform.position;
				Destroy(mainCam.gameObject);
			}

			var camera = GameObject.FindObjectOfType<WeaverCamera>();
			if (camera == null)
			{
				var guids = UnityEditor.AssetDatabase.FindAssets("Game Cameras");
				foreach (var id in guids)
				{
					var path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
					if (path.Contains("WeaverCore.Editor"))
					{
						var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
						if (asset != null)
						{
							camera = GameObject.Instantiate(asset).GetComponentInChildren<WeaverCamera>();
							camera.transform.position = position;
							camera.transform.SetZPosition(-38.1f);
						}
					}
				}
			}
			_instance = camera;
			DontDestroyOnLoad(_instance.GetComponentInParent<GameCameras>());
		}
#endif


		static WeaverCamera _instance;
		public static WeaverCamera Instance
		{
			get
			{
				if (_instance == null)
				{
#if UNITY_EDITOR
					EditorInit();
					//_instance = GameObject.FindObjectOfType<WeaverCamera>();
#else
					throw new Exception("The WeaverCamera has not been created yet. The WeaverCamera will be created when the main menu is loaded");
#endif
				}
				return _instance;
			}
		}


		/*[OnRuntimeInit]
		static void Init()
		{
			Debug.Log("INIT CAMERA");
			_instance = staticImpl.Create();
			if (_instance == null)
			{
				UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
			}
			else
			{
				_instance.Initialize();
			}
		}

		static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
			Init();
		}

		private static WeaverCamera _instance = null;
		bool initialized = false;

		static WeaverCam_I.Statics staticImpl = ImplFinder.GetImplementation<WeaverCam_I.Statics>();
		WeaverCam_I impl;*/


		//public CameraShaker Shaker { get; private set; }
		/*public static WeaverCamera Instance
		{
			get
			{
				if (_instance == null)
				{
					Init();
				}
				return _instance;

			}
		}*/

		//WeaverCam_I impl;

		static List<CameraExtension> featuresToAdd;

		[OnFeatureLoad]
		static void OnFeatureLoad(CameraExtension feature)
		{
			if (_instance == null)
			{
				if (featuresToAdd == null)
				{
					featuresToAdd = new List<CameraExtension>();
				}
				featuresToAdd.Add(feature);
			}
			else
			{
				Instantiate(feature, _instance.transform);
			}
		}

		[OnFeatureUnload]
		static void OnFeatureUnload(CameraExtension feature)
		{
			if (_instance == null)
			{
				featuresToAdd.Remove(feature);
			}
			else
			{
				var components = _instance.transform.GetComponentsInChildren(feature.GetType());
				for (int i = components.GetLength(0) - 1; i >= 0; i--)
				{
					if (components[i] != null && components[i].transform.parent == _instance.transform)
					{
						GameObject.Destroy(components[i].gameObject);
					}
				}
			}
		}

		void Awake()
		{
			//WeaverLog.Log("WEAVER CAMERA CREATED");
			if (_instance != null && _instance != this)
			{
				return;
				//throw new Exception("Cannot have more than one WeaverCamera in the game");
			}
			_instance = this;

			/*if (Initialization.Environment == Enums.RunningState.Game)
			{
				Debug.Log("Obj = " + transform);
				Debug.Log("Parent = " + transform?.parent);
				Debug.Log("Grandparent = " + transform?.parent?.parent);
				Debug.Log("HudCamera = " + transform?.parent?.parent?.Find("HudCamera"));
				Debug.Log("HudCamera OBJ = " + transform?.parent?.parent?.Find("HudCamera")?.gameObject);
				var hudCamera = transform.parent.parent.Find("HudCamera");
				Debug.Log("Adding Rect Transform");
				Debug.Log("GM_A = " + hudCamera?.gameObject?.name);
				hudCamera.gameObject.AddComponent<RectTransform>();
				Debug.Log("Adding SetCameraRect");
				Debug.Log("GM_B = " + hudCamera?.gameObject?.name);
				hudCamera.gameObject.AddComponent<SetCameraRect>();
				Debug.Log("A");
					//TODO TODO TODO - ADD RECT TRANSFORM AND SetCameraRect Components
			}
			Debug.Log("B");*/
			/*if (GameCameras.instance == null)
			{
				new GameObject("Game Cameras").AddComponent<GameCameras>();
			}*/
			/*if (Initialization.Environment == Enums.RunningState.Editor && GetComponent<CameraController>() == null)
			{
				gameObject.AddComponent<CameraController>();
			}*/
			//impl = (WeaverCam_I)gameObject.AddComponent(ImplFinder.GetImplementationType<WeaverCam_I>());
			/*if (transform.parent == null)
			{
				var parentObject = new GameObject("Camera Parent");
				parentObject.transform.position = transform.position;
				//Shaker = parentObject.AddComponent<CameraShaker>();
				//parentObject.AddComponent<CameraShaker>();
				transform.parent = parentObject.transform;
				transform.localPosition = Vector3.zero;
			}*/
			//else
			//{
			//WeaverLog.Log("BOTTOM PATH");
			/*if (transform.parent.GetComponent<CameraShaker>() == null)
			{
				//WeaverLog.Log("ADDING SHAKER");
				transform.parent.gameObject.AddComponent<CameraShaker>();
			}*/
			/*if ((Shaker = transform.parent.GetComponent<CameraShaker>()) == null)
			{
				Shaker = transform.parent.gameObject.AddComponent<CameraShaker>();
			}*/
			//}

			//impl.Initialize();

			//Debug.Log("CAMERA LOADED");
			//Debug.Log("C");
			ReflectionUtilities.ExecuteMethodsWithAttribute<AfterCameraLoadAttribute>();

			//Debug.Log("D");
			if (featuresToAdd != null)
			{
				//Debug.Log("E");
				foreach (var feature in featuresToAdd)
				{
					//Debug.Log("F");
					Instantiate(feature, transform);
				}
				//Debug.Log("G");
				featuresToAdd = null;
			}
			//Debug.Log("H");
			/*foreach (var feature in Registry.GetAllFeatures<CameraExtension>())
			{
				Instantiate(feature, transform);
			}*/
		}

		/*void Initialize()
		{
			if (initialized)
			{
				return;
			}

			impl = (WeaverCam_I)gameObject.AddComponent(ImplFinder.GetImplementationType<WeaverCam_I>());

			if (transform.parent == null)
			{
				var parentObject = new GameObject("Camera Parent");
				parentObject.transform.position = transform.position;
				Shaker = parentObject.AddComponent<CameraShaker>();
				transform.parent = parentObject.transform;
				transform.localPosition = Vector3.zero;
			}
			else
			{
				if ((Shaker = transform.parent.GetComponent<CameraShaker>()) == null)
				{
					Shaker = transform.parent.gameObject.AddComponent<CameraShaker>();
				}
			}

			impl.Initialize();

			foreach (var feature in Registry.GetAllFeatures<CameraExtension>())
			{
				Instantiate(feature, transform);
			}

			initialized = true;
		}*/
	}
}
