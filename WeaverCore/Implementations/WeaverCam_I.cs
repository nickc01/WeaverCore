using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Enums;

namespace WeaverCore.Implementations
{
	public abstract class WeaverCam_I : MonoBehaviour, IImplementation
	{
		[HideInInspector]
		public WeaverCam Cam;

		public virtual void Initialize()
		{
			
		}


		public abstract class Statics : IImplementation
		{
			public abstract WeaverCam Create();
		}

		public abstract class Shaker_I : MonoBehaviour, IImplementation
		{
			public abstract void Shake(Vector3 amount, float duration, int priority = 100);
			public abstract void Shake(ShakeType type);
			public abstract void SetRumble(Vector3 amount);
			public abstract void SetRumble(RumbleType type);
			public abstract void StopRumbling();
		}
	}
}
