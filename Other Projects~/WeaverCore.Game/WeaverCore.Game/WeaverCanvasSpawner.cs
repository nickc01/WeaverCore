using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game
{
	/*class WeaverCanvasSpawner : IInit
	{
		static bool added = false;

		public void OnInit()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
			var canvasObject = new GameObject();

			var canvasPrefab = WeaverAssetLoader.LoadWeaverAsset<GameObject>("Weaver Canvas");
			var canvas = GameObject.Instantiate(canvasPrefab, null).GetComponent<Canvas>();

			GameObject.DontDestroyOnLoad(canvas.gameObject);

			foreach (var extension in Registry.GetAllFeatures<CanvasExtension>())
			{
				GameObject.Instantiate(extension.gameObject, canvas.transform);
			}
		}
	}*/
}
