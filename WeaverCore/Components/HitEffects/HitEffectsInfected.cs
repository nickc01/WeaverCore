using System;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;
using WeaverCore.Enums;

namespace WeaverCore.Components.HitEffects
{
	[RequireComponent(typeof(SpriteFlasher))]
	public class HitEffectsInfected : MonoBehaviour, IHitEffects
	{
		//protected static ObjectPool DamageFlashPool;
		//protected static ObjectPool OrangeSpatterPool;
		//protected static ObjectPool HitPuffPool;

		[SerializeField]
		bool doFlashEffects = true;
		[SerializeField]
		bool doBlood = true;
		bool firedOnCurrentFrame = false;
		SpriteFlasher flasher;


		[Space]
		[Header("Customization")]
		[SerializeField]
		AudioClip _damageSound;
		public AudioClip DamageSound
		{
			get
			{
				return _damageSound;
			}
			protected set
			{
				_damageSound = value;
			}
		}

		[SerializeField]
		float audioPitchMin = 0.75f;
		[SerializeField]
		float audioPitchMax = 1.25f;

		[SerializeField]
		GameObject _damageFlash;
		public GameObject DamageFlash
		{
			get
			{
				return _damageFlash;
			}
			protected set
			{
				_damageFlash = value;
			}
		}

		[SerializeField]
		GameObject _orangeSpatter;
		public GameObject OrangeSpatter
		{
			get
			{
				return _orangeSpatter;
			}
			protected set
			{
				_orangeSpatter = value;
			}
		}

		[SerializeField]
		GameObject _hitPuff;
		public GameObject HitPuff
		{
			get
			{
				return _hitPuff;
			}
			protected set
			{
				_hitPuff = value;
			}
		}

		//static AudioClip DamageSound;

		protected virtual void Reset()
		{
			//Debug.Log("RESET");
			DamageSound = Assets.AudioAssets.DamageEnemy;
			DamageFlash = WeaverAssets.LoadWeaverAsset<GameObject>("Hit Flash Orange");
			OrangeSpatter = WeaverAssets.LoadWeaverAsset<GameObject>("Spatter Orange");
			HitPuff = WeaverAssets.LoadWeaverAsset<GameObject>("Hit Puff");
		}

		protected virtual void Start()
		{
			//DamageFlashPool = new ObjectPool(DamageFlash);
			//OrangeSpatterPool = new ObjectPool(OrangeSpatter);
			//HitPuffPool = new ObjectPool(HitPuff);
			/*if (InfectedHitPool == null && )
			{
				InfectedHitPool = new ObjectPool(Assets.EffectAssets.UninfectedHitPrefab, PoolLoadType.Local);
			}*/
			flasher = GetComponent<SpriteFlasher>();
		}

		protected virtual void Update()
		{
			firedOnCurrentFrame = false;
		}

		public void PlayHitEffect(HitInfo hit, Vector3 effectsOffset = default(Vector3))
		{
			if (!firedOnCurrentFrame)
			{
				firedOnCurrentFrame = true;

				var audio = WeaverAudio.PlayAtPoint(DamageSound, transform.position, channel: AudioChannel.Sound);
				audio.AudioSource.pitch = UnityEngine.Random.Range(audioPitchMin,audioPitchMax);

				//DamageFlashPool.Instantiate(transform.position + effectsOffset, Quaternion.identity);
				Pooling.Instantiate(DamageFlash, transform.position + effectsOffset, Quaternion.identity);

				if (doFlashEffects)
				{
					flasher.flashInfected();
				}

				switch (DirectionUtilities.DegreesToDirection(hit.Direction))
				{
					case CardinalDirection.Up:
						if (doBlood)
						{
							Blood.SpawnDirectionalBlood(transform.position + effectsOffset, CardinalDirection.Up);
						}
						//HitPuffPool.Instantiate(transform.position, Quaternion.Euler(270f, 90f, 270f));
						Pooling.Instantiate(HitPuff, transform.position, Quaternion.Euler(270f, 90f, 270f));
						break;
					case CardinalDirection.Down:
						if (doBlood)
						{
							Blood.SpawnDirectionalBlood(transform.position + effectsOffset, CardinalDirection.Down);
						}
						Pooling.Instantiate(HitPuff, transform.position, Quaternion.Euler(-72.5f, -180f, -180f));
						break;
					case CardinalDirection.Left:
						if (doBlood)
						{
							Blood.SpawnDirectionalBlood(transform.position + effectsOffset, CardinalDirection.Left);
						}
						Pooling.Instantiate(HitPuff, transform.position, Quaternion.Euler(180f, 90f, 270f));
						break;
					case CardinalDirection.Right:
						if (doBlood)
						{
							Blood.SpawnDirectionalBlood(transform.position + effectsOffset,CardinalDirection.Right);
						}
						Pooling.Instantiate(HitPuff, transform.position, Quaternion.Euler(0f, 90f, 270f));
						break;
				}

				//GameObject hitParticles = Instantiate(Assets.EffectAssets.UninfectedHitPrefab, transform.position + effectsOffset, Quaternion.identity);
				//GameObject hitParticles = InfectedHitPool.Instantiate(transform.position + effectsOffset, Quaternion.identity);

				/*var direction = DirectionUtilities.DegreesToDirection(hit.Direction);

				switch (direction)
				{
					case CardinalDirection.Up:
						SetRotation2D(hitParticles.transform, 45f);
						break;
					case CardinalDirection.Down:
						SetRotation2D(hitParticles.transform, 225f);
						break;
					case CardinalDirection.Left:
						SetRotation2D(hitParticles.transform, -225f);
						break;
					case CardinalDirection.Right:
						SetRotation2D(hitParticles.transform, -45f);
						break;
				}

				Flings.SpawnFlings(NormalFlings, transform.position + effectsOffset, direction);*/
			}
		}

		/*static void SetRotation2D(Transform t, float rotation)
		{
			Vector3 eulerAngles = t.eulerAngles;
			eulerAngles.z = rotation;
			t.eulerAngles = eulerAngles;
		}*/
	}
}
