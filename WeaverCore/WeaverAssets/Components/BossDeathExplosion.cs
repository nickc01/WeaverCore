using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	public class BossDeathExplosion : MonoBehaviour
	{
		public bool ShakeCamera;
		public bool SpawnBloodParticles;
		public bool PlayExplosionSound;


		public void Start()
		{
			if (ShakeCamera)
			{
				CameraShaker.Instance.Shake(Enums.ShakeType.BigShake);
			}
			if (SpawnBloodParticles)
			{
				//TODO : SPAWN BLOOD PARTICLES
			}
			if (PlayExplosionSound)
			{
				GetComponent<AudioSource>().Play();
			}
		}
	}
}
