using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public class WeaverCam : MonoBehaviour
	{
		private static WeaverCam _instance = null;
		bool initialized = false;

		static WeaverCam_I.Statics staticImpl = ImplFinder.GetImplementation<WeaverCam_I.Statics>();
		WeaverCam_I impl;


		public CameraShaker Shaker { get; private set; }
		public static WeaverCam Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = staticImpl.Create();
					_instance.Initialize();
				}
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

			initialized = true;
		}
	}
}
