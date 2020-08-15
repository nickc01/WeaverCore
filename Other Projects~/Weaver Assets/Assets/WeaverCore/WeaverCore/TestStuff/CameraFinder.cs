using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.TestStuff
{
	/*class CameraFinder : IInit
	{
		public void OnInit()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		}

		private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
		{
			UnboundCoroutine.Start(CameraSearch());
		}



		static IEnumerator CameraSearch()
		{
			yield return new WaitForSeconds(0.5f);
			foreach (var cam in GameObject.FindObjectsOfType<Camera>())
			{
				WeaverLog.Log("Cameras Found = " + cam.name);
			}
		}



	}*/
}
