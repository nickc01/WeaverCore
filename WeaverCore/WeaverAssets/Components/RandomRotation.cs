using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Sets the object to a randomized rotation
	/// </summary>
	public class RandomRotation : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The minimum rotation angle")]
		float randomRotationMin = 0f;

		[SerializeField]
		[Tooltip("The maximum rotation angle")]
		float randomRotationMax = 360f;

		void OnEnable()
		{
			transform.localEulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, UnityEngine.Random.Range(0f, 360f));
		}
	}
}
