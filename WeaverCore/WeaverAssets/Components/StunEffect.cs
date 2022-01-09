using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// An effect that is instantiated when an enemy is stunned
	/// </summary>
    public class StunEffect : MonoBehaviour
	{
		static ObjectPool StunEffectPool;

		PoolableObject poolComponent;

		[SerializeField]
		[Tooltip("The sound effect that is played when stunned")]
		AudioClip StunSound;

		void Start()
		{
			if (StunSound != null)
			{
				WeaverAudio.PlayAtPoint(StunSound, transform.position);
			}
			transform.localRotation = Quaternion.Euler(0f,0f,Random.Range(0f,360f));
			CameraShaker.Instance.Shake(Enums.ShakeType.AverageShake);
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

		/// <summary>
		/// Spawns a stun effect
		/// </summary>
		/// <param name="position">The position to spawn the effect</param>
		public static void Spawn(Vector3 position)
		{
			Spawn_Internal(position, null, false);
		}

		/// <summary>
		/// Spawns a stun effect
		/// </summary>
		/// <param name="position">The position to spawn the effect</param>
		/// <param name="clip">The sound to play when stunned</param>
		public static void Spawn(Vector3 position, AudioClip clip)
		{
			Spawn_Internal(position, clip, true);
		}

		static void Spawn_Internal(Vector3 position, AudioClip clip, bool playProvidedAudioClip)
		{
			if (StunEffectPool == null)
			{
				StunEffectPool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Stun Effect"));
			}
			var instance = StunEffectPool.Instantiate(position, Quaternion.identity).GetComponent<StunEffect>();

			if (playProvidedAudioClip)
			{
				instance.StunSound = clip;
			}
		}
	}
}
