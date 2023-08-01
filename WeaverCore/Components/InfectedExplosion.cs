using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	public class InfectedExplosion : MonoBehaviour, IOnPool
	{
		[SerializeField]
		AudioClip ExplosionSound;
		[SerializeField]
		float explosionPitchMin = 0.85f;
		[SerializeField]
		float explosionPitchMax = 1.1f;
		[SerializeField]
		float deathWaveScale = 3f;
		[SerializeField]
		OnDoneBehaviour whenDone = OnDoneBehaviour.DestroyOrPool;

		static ObjectPool ExplosionPool;

		new Collider2D collider;

		public float DefaultScale { get; private set; }

		[NonSerialized]
		[ExcludeFieldFromPool]
		ParticleSystem _particles;

		public ParticleSystem Particles
		{
			get
			{
				if (_particles == null)
				{
					_particles = GetComponentInChildren<ParticleSystem>();
				}
				return _particles;
			}
		}

		void OnEnable()
		{
			if (collider == null)
			{
				collider = GetComponent<Collider2D>();
			}
			collider.enabled = true;
			var audio = WeaverAudio.PlayAtPoint(ExplosionSound, transform.position);
			audio.AudioSource.pitch = UnityEngine.Random.Range(explosionPitchMin, explosionPitchMax);

			CameraShaker.Instance.Shake(ShakeType.AverageShake);
			DeathWave.Spawn(transform.position, 0.5f);
			StartCoroutine(Waiter());
		}

		IEnumerator Waiter()
		{
			yield return new WaitForSeconds(0.5f);
			collider.enabled = false;
			yield return new WaitForSeconds(1f);
			whenDone.DoneWithObject(this);
		}

		public static InfectedExplosion Spawn(Vector3 position)
		{
			return Spawn(position, 1f);
		}

        public static InfectedExplosion Spawn(Vector3 position, float scale)
		{
            if (ExplosionPool == null)
            {
                ExplosionPool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Infected Explosion"));
            }
            var instance = ExplosionPool.Instantiate<InfectedExplosion>(position, Quaternion.identity);

			instance.DefaultScale = instance.transform.GetXLocalScale();

			instance.transform.SetLocalScaleXY(scale, scale);

            var emission = instance.Particles.emission;
			//Debug.Log("EMISSION MULTIPLIER = " + emission.rateOverTimeMultiplier);
			emission.rateOverTimeMultiplier = 1000f * scale;

            return instance;
        }

        void IOnPool.OnPool()
        {
			transform.SetLocalScaleXY(DefaultScale, DefaultScale);
			var emission = Particles.emission;

			emission.rateOverTimeMultiplier = 1000f;
        }
    }
}
