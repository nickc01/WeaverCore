using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Automatically sends an object back to it's pool when the particle system on the object is completed
    /// </summary>
    [RequireComponent(typeof(PoolableObject))]
	[RequireComponent(typeof(ParticleSystem))]
	public class PoolOnParticlesDone : MonoBehaviour
	{
		ParticleSystem particles;
		PoolableObject poolComponent;

		void Awake()
		{
			particles = GetComponent<ParticleSystem>();
			var mainModule = particles.main;
			mainModule.stopAction = ParticleSystemStopAction.Callback;

			poolComponent = GetComponent<PoolableObject>();
		}

		void OnParticleSystemStopped()
		{
			poolComponent.ReturnToPool();
		}
	}
}
