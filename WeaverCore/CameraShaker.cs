using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore
{
    public class CameraShaker : MonoBehaviour
	{
		/*static CameraShaker _instance;
		public static CameraShaker Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = CameraShaker.Instance;
				}
				return _instance;
			}
		}*/

		[AfterCameraLoad(int.MinValue)]
		static void Init()
		{
			if (WeaverCamera.Instance.transform.parent.GetComponent<CameraShaker>() == null)
			{
				WeaverCamera.Instance.transform.parent.gameObject.AddComponent<CameraShaker>();
			}
		}

		public static CameraShaker Instance { get; private set; }

		Shaker_I impl;

		void Awake()
		{
			Instance = this;
			impl = (Shaker_I)gameObject.AddComponent(ImplFinder.GetImplementationType<Shaker_I>());
		}

		public void Shake(Vector3 amount, float duration, int priority = int.MaxValue)
		{
			impl.Shake(amount, duration, priority);
		}
		public void Shake(ShakeType type)
		{
			impl.Shake(type);
		}
		public void SetRumble(Vector3 amount)
		{
			impl.SetRumble(amount);
		}
		public void SetRumble(RumbleType type)
		{
			impl.SetRumble(type);
		}
	}
}
