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
	/// <summary>
	/// A component used to access and manipulate the camera
	/// </summary>
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

		/// <summary>
		/// Gets the currently instantiated camera in the game
		/// </summary>
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

		GameCameras _cameras;

		public GameCameras Cameras
        {
			get
            {
                if (_cameras == null)
                {
					_cameras = transform.parent.parent.GetComponent<GameCameras>();
                }
				return _cameras;
            }
        }

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
			if (_instance != null && _instance != this)
			{
				return;
			}
			_instance = this;
			ReflectionUtilities.ExecuteMethodsWithAttribute<AfterCameraLoadAttribute>();

			if (featuresToAdd != null)
			{
				foreach (var feature in featuresToAdd)
				{
					Instantiate(feature, transform);
				}
				featuresToAdd = null;
			}
		}
	}
}
