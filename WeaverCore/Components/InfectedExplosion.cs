using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	public class InfectedExplosion : MonoBehaviour
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

		void OnEnable()
		{
			if (collider == null)
			{
				collider = GetComponent<Collider2D>();
			}
			collider.enabled = true;
			var audio = WeaverAudio.PlayAtPoint(ExplosionSound, transform.position);
			audio.AudioSource.pitch = Random.Range(explosionPitchMin, explosionPitchMax);

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
			if (ExplosionPool == null)
			{
				ExplosionPool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Infected Explosion"));
			}
			return ExplosionPool.Instantiate<InfectedExplosion>(position, Quaternion.identity);
		}
	}
}
