using System;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;
using WeaverCore.Enums;

namespace WeaverCore.Components.HitEffects
{
	/// <summary>
	/// The hit effects for infected enemies
	/// </summary>
	//[RequireComponent(typeof(SpriteFlasher))]
	public class HitEffectsInfected : MonoBehaviour, IHitEffects
	{
		[SerializeField]
		[Tooltip("Should the sprite flash when hit?")]
		bool doFlashEffects = true;
		[SerializeField]
		[Tooltip("Should blood particles be emitted when hit?")]
		bool doBlood = true;
		bool firedOnCurrentFrame = false;
		//SpriteFlasher flasher;
		SpriteFlasher[] flashers;


		[Space]
		[Header("Customization")]
		[SerializeField]
		[Tooltip("The sound that is played when damaged")]
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
		[Tooltip("The minimum pitch of the damage sound")]
		float audioPitchMin = 0.75f;
		[SerializeField]
		[Tooltip("The maximum pitch of the damage sound")]
		float audioPitchMax = 1.25f;

		[SerializeField]
		[Tooltip("The flash effect that is played when hit")]
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
		[Tooltip("The splatter particles emitted when hit")]
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
		[Tooltip("The puff particle effect emitted when hit")]
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
		protected virtual void Reset()
		{
			DamageSound = Assets.AudioAssets.DamageEnemy;
			DamageFlash = WeaverAssets.LoadWeaverAsset<GameObject>("Hit Flash Orange");
			OrangeSpatter = WeaverAssets.LoadWeaverAsset<GameObject>("Spatter Orange");
			HitPuff = WeaverAssets.LoadWeaverAsset<GameObject>("Hit Puff");
		}

		protected virtual void Start()
		{
			flashers = GetComponentsInChildren<SpriteFlasher>();
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
				Pooling.Instantiate(DamageFlash, transform.position + effectsOffset, Quaternion.identity);

				if (doFlashEffects)
				{
                    foreach (var flasher in flashers)
                    {
						if (flasher != null && flasher.isActiveAndEnabled)
						{
                            flasher.flashInfected();
                        }
                    }
					//flasher.flashInfected();
				}

				switch (DirectionUtilities.DegreesToDirection(hit.Direction))
				{
					case CardinalDirection.Up:
						if (doBlood)
						{
							Blood.SpawnDirectionalBlood(transform.position + effectsOffset, CardinalDirection.Up);
						}
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
			}
		}
	}
}
