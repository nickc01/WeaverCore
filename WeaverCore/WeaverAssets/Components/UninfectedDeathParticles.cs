using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// A particle system that is created when an uninfected enemy dies
	/// </summary>
	public class UninfectedDeathParticles : MonoBehaviour
	{
		void Start()
		{
			Destroy(gameObject, 5);
		}
	}
}
