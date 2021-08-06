using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
			var mainCam = Camera.main;
			if (mainCam != null)
			{
				if (mainCam.GetComponent<WeaverCamera>() == null)
				{
					mainCam.gameObject.AddComponent<WeaverCamera>();
				}
			}
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
					_instance = GameObject.FindObjectOfType<WeaverCamera>();
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
			if (_instance != null)
			{
				throw new Exception("Cannot have more than one WeaverCamera in the game");
			}
			_instance = this;
			if (GameCameras.instance == null)
			{
				new GameObject("Game Cameras").AddComponent<GameCameras>();
			}
			if (Initialization.Environment == Enums.RunningState.Editor && GetComponent<CameraController>() == null)
			{
				gameObject.AddComponent<CameraController>();
			}
			//impl = (WeaverCam_I)gameObject.AddComponent(ImplFinder.GetImplementationType<WeaverCam_I>());
			if (transform.parent == null)
			{
				var parentObject = new GameObject("Camera Parent");
				parentObject.transform.position = transform.position;
				//Shaker = parentObject.AddComponent<CameraShaker>();
				//parentObject.AddComponent<CameraShaker>();
				transform.parent = parentObject.transform;
				transform.localPosition = Vector3.zero;
			}
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

			Debug.Log("CAMERA LOADED");
			ReflectionUtilities.ExecuteMethodsWithAttribute<AfterCameraLoadAttribute>();

			if (featuresToAdd != null)
			{
				foreach (var feature in featuresToAdd)
				{
					Instantiate(feature, transform);
				}
				featuresToAdd = null;
			}
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
