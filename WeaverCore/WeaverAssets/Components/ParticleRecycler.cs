using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
    [RequireComponent(typeof(ParticleSystem))]
	[RequireComponent(typeof(PoolableObject))]
	public class ParticleRecycler : MonoBehaviour
	{
		ParticleSystem particles;
		ParticleSystem.MainModule main;

		PoolableObject pool;

		void Start()
		{
			particles = GetComponent<ParticleSystem>();
			pool = GetComponent<PoolableObject>();
			main = particles.main;

			main.stopAction = ParticleSystemStopAction.Callback;
		}

		void OnParticleSystemStopped()
		{
			pool = GetComponent<PoolableObject>();
			if (pool != null)
			{
				pool.ReturnToPool();
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}
