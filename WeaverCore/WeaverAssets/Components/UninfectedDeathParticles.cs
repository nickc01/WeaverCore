using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	public class UninfectedDeathParticles : MonoBehaviour
	{
		void Start()
		{
			Destroy(gameObject, 5);
		}
	}
}
