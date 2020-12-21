using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class StunEffect : MonoBehaviour
	{
		static ObjectPool StunEffectPool;


		PoolableObject poolComponent;

		[SerializeField]
		AudioClip StunSound;

		void Start()
		{
			if (StunSound != null)
			{
				Debug.Log("PLAYING STUN SOUND!!!");
				WeaverAudio.PlayAtPoint(StunSound, transform.position);
			}
			transform.localRotation = Quaternion.Euler(0f,0f,Random.Range(0f,360f));
			WeaverCam.Instance.Shaker.Shake(Enums.ShakeType.AverageShake);
			WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset4);
			poolComponent = GetComponent<PoolableObject>();
			if (poolComponent != null)
			{
				poolComponent.ReturnToPool(0.1f);
			}
			else
			{
				Destroy(gameObject, 0.1f);
			}
		}


		public static void Spawn(Vector3 position)
		{
			Spawn_Internal(position, null, false);
		}

		public static void Spawn(Vector3 position, AudioClip clip)
		{
			Spawn_Internal(position, clip, true);
		}

		static void Spawn_Internal(Vector3 position, AudioClip clip, bool playProvidedAudioClip)
		{
			if (StunEffectPool == null)
			{
				StunEffectPool = new ObjectPool(WeaverAssets.LoadWeaverAsset<GameObject>("Stun Effect"));
			}
			var instance = StunEffectPool.Instantiate(position, Quaternion.identity).GetComponent<StunEffect>();

			if (playProvidedAudioClip)
			{
				instance.StunSound = clip;
			}
		}
	}
}
