using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Assets.Components
{

    /// <summary>
    /// Plays the Boss Death Explosion, including shaking the camera and spawning particles
    /// </summary>
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
				Blood.SpawnRandomBlood(transform.position);
			}
			if (PlayExplosionSound)
			{
				GetComponent<AudioSource>().Play();
			}
		}
	}
}
