using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	public class RandomRotation : MonoBehaviour
	{
		[SerializeField]
		float randomRotationMin = 0f;

		[SerializeField]
		float randomRotationMax = 360f;

		void OnEnable()
		{
			transform.localEulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, UnityEngine.Random.Range(0f, 360f));
		}
	}
}
