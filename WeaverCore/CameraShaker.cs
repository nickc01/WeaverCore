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

    /// <summary>
    /// Used to shake the camera
    /// </summary>
    public class CameraShaker : MonoBehaviour
	{
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

		/// <summary>
		/// Shakes the camera
		/// </summary>
		/// <param name="amount">How much the camera should shake</param>
		/// <param name="duration">How long the camera should shake for</param>
		/// <param name="priority">The priority of this action. The higher the priority, the more likely this shake action will be used instead of other shake actions</param>
		public void Shake(Vector3 amount, float duration, int priority = int.MaxValue)
		{
			impl.Shake(amount, duration, priority);
		}
		/// <summary>
		/// Shakes the camera
		/// </summary>
		/// <param name="type">The type of shake to be applied</param>
		public void Shake(ShakeType type)
		{
			impl.Shake(type);
		}

		/// <summary>
		/// Causes the camera to have a rumble effect
		/// </summary>
		/// <param name="amount">The amount of rumble to be applied</param>
		public void SetRumble(Vector3 amount)
		{
			impl.SetRumble(amount);
		}

		/// <summary>
		/// Causes the camera to have a rumble effect
		/// </summary>
		/// <param name="type">The type of rumble effect to apply</param>
		public void SetRumble(RumbleType type)
		{
			impl.SetRumble(type);
		}
	}
}
