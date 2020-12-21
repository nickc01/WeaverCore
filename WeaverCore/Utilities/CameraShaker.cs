using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	public class CameraShaker : MonoBehaviour
	{

		WeaverCam_I.Shaker_I impl;

		void Awake()
		{
			impl = (WeaverCam_I.Shaker_I)gameObject.AddComponent(ImplFinder.GetImplementationType<WeaverCam_I.Shaker_I>());
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
