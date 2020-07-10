using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public class WeaverCam : MonoBehaviour
	{
		class RInit : IRuntimeInit
		{
			public void RuntimeInit()
			{
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

			private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
			{
				UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
				RuntimeInit();
			}
		}

		private static WeaverCam _instance = null;
		bool initialized = false;

		static WeaverCam_I.Statics staticImpl = ImplFinder.GetImplementation<WeaverCam_I.Statics>();
		WeaverCam_I impl;


		public CameraShaker Shaker { get; private set; }
		public static WeaverCam Instance
		{
			get
			{
				return _instance;

			}
		}

		void Initialize()
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
		}

		/*public PostProcessLayer AddPostProcessLayer(LayerMask mask)
		{
			var ppl = gameObject.AddComponent<PostProcessLayer>();
			ppl.volumeLayer = mask;
			ppl.

			return ppl;
		}*/
	}
}
